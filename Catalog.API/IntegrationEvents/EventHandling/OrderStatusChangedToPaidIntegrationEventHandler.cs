using System.Threading.Tasks;
using Catalog.API.Infrastructure;
using Catalog.API.IntegrationEvents.Events;
using eShop.EventBus.Abstractions;
using Microsoft.Extensions.Logging;

namespace Catalog.API.IntegrationEvents.EventHandling
{
    public class OrderStatusChangedToPaidIntegrationEventHandler(
        CatalogContext catalogContext,
        ILogger<OrderStatusChangedToAwaitingValidationIntegrationEventHandler> logger
        ) : IIntegrationEventHandler<OrderStatusChangedToPaidIntegrationEvent>
    {
        public async Task Handle(OrderStatusChangedToPaidIntegrationEvent @event)
        {
            logger.LogInformation("Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);

            //we're not blocking stock/inventory
            foreach (var orderStockItem in @event.OrderStockItems)
            {
                var catalogItem = catalogContext.CatalogItems.Find(orderStockItem.ProductId);

                catalogItem.RemoveStock(orderStockItem.Units);
            }

            await catalogContext.SaveChangesAsync();
        }
    }
}
