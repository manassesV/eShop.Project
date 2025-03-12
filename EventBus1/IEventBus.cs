using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventBus1.Events;

namespace EventBus1
{
    public interface IEventBus
    {
        Task PublishAsync(IntegrationEvent @event);
    }
}
