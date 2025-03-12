﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EventBus1;
using EventBus1.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace EventBusRabbitMQ1
{
    public sealed class RabbitMQEventBus(
        ILogger<RabbitMQEventBus> logger,
        IServiceProvider serviceProvider,
        IOptions<EventBusOptions> options,
        IOptions<EventBusSubscriptionInfo> subscriptionOption,
        RabbitMQTelemetry rabbitMQTelemetry
        ) : IEventBus, IDisposable, IHostedService
    {


        private const string ExchangeName = "eshop_event_bus";

        private readonly ResiliencePipeline _pipeline = CreateResiliencePipeline(options.Value.RetryCount);
        private readonly TextMapPropagator _propagator = rabbitMQTelemetry.Propagator;
        private readonly ActivitySource _activitySource = rabbitMQTelemetry.activitySource;
        private readonly string _queueName = options.Value.SubscrriptionClientName;
        private readonly EventBusSubscriptionInfo _subscriptionInfo = subscriptionOption.Value;
        private IConnection _rabbitMQConnection;
        private IModel _consumerChannel;

        private readonly EventHandler<CallbackExceptionEventArgs> CallbackExceptionEventArgs;
        private readonly AsyncEventHandler<BasicDeliverEventArgs> OnMessageReceived;



        public void Dispose()
        {
            _consumerChannel?.Dispose();
        }

        public Task PublishAsync(IntegrationEvent @event)
        {
            var routingKey = @event.GetType().Name;

            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("Creating RabbitMQ channel to publish event: {EventId} ({}EventName)", @event.Id, routingKey);
            }

            using var channel = _rabbitMQConnection?.CreateModel() ?? throw new InvalidOperationException("RabbitMQQ connection is not open");

            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("Declaring RabbitMQ exchange to publish event: {EventId}", @event.Id);
            }

            channel.ExchangeDeclare(exchange: ExchangeName, type: "direct");

            var body = SerializeMessage(@event);

            // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
            // https://github.com/open-telemetry/semantic-conventions/blob/main/docs/messaging/messaging-spans.md
            var activityName = $"{routingKey} publish";


            return _pipeline.Execute(() =>
            {
                using var activity = _activitySource.StartActivity(activityName, ActivityKind.Client);

                // Depending on Sampling (and whether a listener is registered or not), the activity above may not be created.
                // If it is created, then propagate its context. If it is not created, the propagate the Current context, if any.

                ActivityContext contextToInject = default;

                if(activity != null)
                {
                    contextToInject = activity.Context;
                }else if(Activity.Current != null){
                    contextToInject = Activity.Current.Context;
                }

                var properties = channel.CreateBasicProperties();

                properties.DeliveryMode = 2;

                static void InjectTraceContextIntoBasicProperties(IBasicProperties props,
                    string key,
                    string values)
                {
                    props.Headers ??= new Dictionary<string, object>();
                    props.Headers.Add(key, values);
                }

                _propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), properties, InjectTraceContextIntoBasicProperties);

                SetActivityContext(activity, routingKey, "publish");

                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.LogTrace("Publishing event to RabbitMQ: {EventId}", @event.Id);
                }

                try
                {

                   var publish = channel.CreateBasicPublishBatch();
                    publish.Add(exchange: ExchangeName, 
                               routingKey: routingKey, 
                               mandatory: true,
                               properties: properties,
                               body:body);

                    publish.Publish();

                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    activity.SetExceptionTags(ex);

                    throw;
                }
            });
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Factory.StartNew(() =>
            {
                logger.LogInformation("Starting RabbitMQ connection on a background thread");

                _rabbitMQConnection = serviceProvider.GetRequiredService<IConnection>();


                if (!_rabbitMQConnection.IsOpen)
                    return;

                if (logger.IsEnabled(LogLevel.Trace))
                    logger.LogTrace("Creating RabbitMQ consumer channel");

                _consumerChannel.CallbackException += CallbackExceptionEventArgs;

                _consumerChannel.ExchangeDeclare(exchange: ExchangeName, type: "direct");

                _consumerChannel.QueueDeclare(queue: _queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                if (logger.IsEnabled(LogLevel.Trace))
                    logger.LogTrace("Starting RabbitMQ basic consume");

               var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
                consumer.Received += OnMessageReceived;

                _consumerChannel.BasicConsume(
                    queue: _queueName,
                    autoAck: false,
                    consumer: consumer);

                foreach(var (eventName, _) in _subscriptionInfo.EventTypes)
                {
                    _consumerChannel.QueueBind(
                        queue: _queueName,
                        exchange: ExchangeName,
                        routingKey: eventName);
                }



            }, TaskCreationOptions.LongRunning);
        }

   
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task OnMessageReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            static IEnumerable<string> ExtractTraceContextFromBasicProperties(IBasicProperties props, string key)
            {
                if(props.Headers.TryGetValue(key, out var value))
                {
                    var bytes = value as byte[];
                    return [Encoding.UTF8.GetString(bytes)];

                }

                return [];
            }

            //Extract the PropationContext of the UpStream parent from the message header
            var parentContext = _propagator.Extract(default, eventArgs.BasicProperties, ExtractTraceContextFromBasicProperties);
            Baggage.Current = parentContext.Baggage;

            // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
            var activityName = $"{eventArgs.RoutingKey} receive";

            using var activity = _activitySource.StartActivity(activityName, ActivityKind.Client, parentContext.ActivityContext);

            SetActivityContext(activity, eventArgs.RoutingKey, "receive");

            var eventName = eventArgs.RoutingKey;
            var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

            try
            {
                activity?.SetTag("message", message);

                if (message.Contains("throw-fake-exception", StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new InvalidOperationException($"Fake exception requested: \"{message}\"");
                }
                await ProcessEvent(eventName, message);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error Processing message \"{Message}\"", message);

                activity.SetExceptionTags(ex);
            }

            _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);

        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("Processing RabbitMQ event: {EventName}", message);
            }

            await using var scope = serviceProvider.CreateAsyncScope();

            if(!_subscriptionInfo.EventTypes.TryGetValue(eventName, out var eventType))
            {
                logger.LogWarning("Unable to resolve event type for event name {EventName}", eventName);
                return;
            }

            //Deserialize the event
            var integrationEvent = DeserializeMessage<IntegrationEvent>(message, eventType);

            foreach (var handler in scope.ServiceProvider.GetKeyedServices<IIntegrationEventHandler>(eventType))
            {
                await handler.Handle(integrationEvent);
            }
        }

        private static ResiliencePipeline CreateResiliencePipeline(int retryCount)
        {
            var retryOptions = new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<BrokerUnreachableException>().Handle<SocketException>(),
                MaxRetryAttempts = retryCount,
                DelayGenerator = (context) => ValueTask.FromResult(GenerateDelay(context.AttemptNumber))
            };

            return new ResiliencePipelineBuilder()
                       .AddRetry(retryOptions)
                       .Build();

            static TimeSpan? GenerateDelay(int attempt)
            {
                return TimeSpan.FromSeconds(Math.Pow(2, attempt));
            }
        }



        [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
            Justification = "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed .NET apps, ensures the JsonSerializer doesn't use Reflection.")]
        [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
        private byte[] SerializeMessage(IntegrationEvent @event)
        {
            return JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType(), _subscriptionInfo.JsonSerializerOptions);

        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
        Justification = "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed .NET apps, ensures the JsonSerializer doesn't use Reflection.")]
        [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
        private T DeserializeMessage<T>(string message, Type eventType) where T : class
        {
            return JsonSerializer.Deserialize(message, eventType, _subscriptionInfo.JsonSerializerOptions) as T;
        }

        private static void SetActivityContext(Activity activity, string routingKey, string operation)
        {
            if (activity is not null)
            {
                activity.SetTag("messaging.system", "rabbitmq");
                activity.SetTag("messaging.destination_kind", "queue");
                activity.SetTag("messaging.operation", operation);
                activity.SetTag("messaging.destination.name", routingKey);
                activity.SetTag("messaging.rabbitmq.routing_key", routingKey);
            }
        }
    }
}
