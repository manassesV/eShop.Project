using System.Collections.Generic;
using eShop.EventBus.Events;

namespace Catalog.API.IntegrationEvents.Events;

    public record OrderStockRejectedIntegrationEvent(int OrderId, List<ConfirmedOrderStockItem> OrderStockItems):IntegrationEvent;
    
