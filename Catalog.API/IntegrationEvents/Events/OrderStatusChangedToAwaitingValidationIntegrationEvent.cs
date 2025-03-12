using System.Collections.Generic;
using eShop.EventBus.Events;

namespace Catalog.API.IntegrationEvents.Events
{
    public record OrderStatusChangedToAwaitingValidationIntegrationEvent(int OrderId, IEnumerable<OrderStockItem> OrderStockItems) : IntegrationEvent;

}
