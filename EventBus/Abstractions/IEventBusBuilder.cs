using Microsoft.Extensions.DependencyInjection;

namespace eShop.EventBus.Abstractions
{
    public interface IEventBusBuilder
    {
        public IServiceCollection Services { get; }
    }
}
