using eShop.EventBus.Events;

namespace PaymentProcessor.IntegrationEvents.Events
{
    public record OrderPaymentSucceededIntegrationEvent(int OrderId):IntegrationEvent;
   
}
