using eShop.EventBus.Events;

namespace Basket.API.IntegrationEvents.Events
{
    public record OrderStartedIntegrationEvent(string UserId): IntegrationEvent;

}
