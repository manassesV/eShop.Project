using System.Threading.Tasks;
using eShop.EventBus.Events;

namespace eShop.EventBus.Abstractions
{
    public interface IEventBus
    {
        Task PublishAsync(IntegrationEvent @event);
    }
}
