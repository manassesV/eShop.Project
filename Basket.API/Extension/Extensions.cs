using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Basket.API.IntegrationEvents.EventHandling;
using Basket.API.IntegrationEvents.Events;
using Basket.API.Repositories;
using eShop.EventBus.Extension;
using eShop.EventBusRabbitMQ;
using eShop.ServiceDefaults;

namespace Basket.API.Extension;

    public static class Extensions
    {
        public static void AddAplicationServices(this IHostApplicationBuilder builder)
        {
            builder.AddDefaultAuthentication();

            builder.AddRedisClient("redis");

            builder.Services.AddSingleton<IBasketRepository, RedisBasketRepository>();

            builder.AddRabbitMqEventBus("eventbus")
                .AddSubscription<OrderStartedIntegrationEvent, OrderStartedIntegrationEventHandler>()
               .ConfigureJsonOptions(options => options.TypeInfoResolverChain.Add(IntegrationEventContext.Default));



        }

    }


[JsonSerializable(typeof(OrderStartedIntegrationEvent))]
partial class IntegrationEventContext : JsonSerializerContext
{

}