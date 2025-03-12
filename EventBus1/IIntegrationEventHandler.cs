using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventBus1.Events;

namespace EventBus1
{
    public interface IIntegrationEventHandler<in TIntegrationEvent>: IIntegrationEventHandler
    where TIntegrationEvent : IntegrationEvent
    {
        Task Handle(TIntegrationEvent @event);

        Task IIntegrationEventHandler.Handle(EventBus1.Events.IntegrationEvent @event) => Handle((TIntegrationEvent)@event);
    }

    public interface IIntegrationEventHandler
    {
        Task Handle(IntegrationEvent @event);
    }
}
