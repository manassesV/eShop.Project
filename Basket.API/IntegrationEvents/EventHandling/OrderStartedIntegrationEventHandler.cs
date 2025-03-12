using System.Collections.Concurrent;
using Basket.API.IntegrationEvents.Events;
using Basket.API.Repositories;
using eShop.EventBus.Abstractions;
using eShop.EventBus.Events;

namespace Basket.API.IntegrationEvents.EventHandling
{
    public class OrderStartedIntegrationEventHandler(
        IBasketRepository repository,
        ILogger<OrderStartedIntegrationEventHandler> logger): IIntegrationEventHandler<OrderStartedIntegrationEvent>
    {
        public async Task Handle(OrderStartedIntegrationEvent @event)
        {
            logger.LogInformation("Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);

            await repository.DeleteBasketAsync(@event.UserId);

        }
    }
}
