using eShop.EventBus.Events;

namespace Catalog.API.IntegrationEvents.Events;

public record OrderStockConfirmedIntegrationEvent(int OrderId): IntegrationEvent;

