using eShop.EventBus.Extension;
using eShop.EventBusRabbitMQ;
using eShop.ServiceDefaults;
using PaymentProcessor;
using PaymentProcessor.EventHandling;
using PaymentProcessor.IntegrationEvents.Events;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddRabbitMqEventBus("EventBus")
    .AddSubscription<OrderStatusChangedToStockConfirmedIntegrationEvent, OrderStatusChangedToStockConfirmedIntegrationEventHandler>();


builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration(nameof(PaymentOptions));

var app = builder.Build();

app.MapDefaultEndpoints();

await app.RunAsync();