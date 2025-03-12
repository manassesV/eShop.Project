using System;
using System.Threading.Tasks;
using eShop.EventBus.Events;

namespace eShop.Ordering.API.Application.IntegrationEvents
{
    public interface IOrderingIntegrationEventService
    {
        Task PublishEventThroughtEventBusAsync(Guid transactionId);

        Task AddAndSaveEventAsync(IntegrationEvent evt);
    }
}
