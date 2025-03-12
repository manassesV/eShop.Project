using System.Threading.Tasks;
using eShop.EventBus.Events;

namespace Catalog.API.IntegrationEvents
{
    public interface ICatalogIntegrationEventService
    {
        Task SaveEventAndCatalogContextChangesAsync(IntegrationEvent evt);

        Task PublishThroughEventBusAsync(IntegrationEvent evt);
    }
}
