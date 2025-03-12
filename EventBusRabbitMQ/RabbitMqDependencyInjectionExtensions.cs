using System;
using eShop.EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace eShop.EventBusRabbitMQ
{
    public static class RabbitMqDependencyInjectionExtensions
    {
        private const string SectionName = "EventBus";

        public static IEventBusBuilder AddRabbitMqEventBus(this IHostApplicationBuilder builder, string connectionName)
        {

            ArgumentNullException.ThrowIfNull(builder);

            builder.AddRabbitMQClient(connectionName, configureConnectionFactory: factory =>
            {
                ((ConnectionFactory)factory).DispatchConsumersAsync = true;
            });

            builder.Services.AddOpenTelemetry()
                .WithTracing(tracing =>
                {
                    tracing.AddSource(RabbitMQTelemetry.ActivitySourceName);

                });

            //Options support
            builder.Services.Configure<EventBusOptions>(builder.Configuration.GetSection(SectionName));

            //Abstractions on top of core client API
            builder.Services.AddSingleton<RabbitMQTelemetry>();
            builder.Services.AddSingleton<IEventBus, RabbitMQEventBus>();

            //Start cosuming messages as soon as the application services
            builder.Services.AddSingleton<IHostedService>(sp => (RabbitMQEventBus)sp.GetRequiredService<IEventBus>());


            return new EventBusBuilder(builder.Services);
        }

        private class EventBusBuilder(IServiceCollection services) : IEventBusBuilder
        {
            public IServiceCollection Services => services;
        }
    }
}
