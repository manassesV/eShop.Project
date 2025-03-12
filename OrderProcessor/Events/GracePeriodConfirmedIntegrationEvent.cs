using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eShop.EventBus.Events;

namespace OrderProcessor.Events
{
    public record GracePeriodConfirmedIntegrationEvent:IntegrationEvent
    {
        public int OrderId { get; }
        public GracePeriodConfirmedIntegrationEvent(int orderid) =>
              OrderId = orderid;
            
        
    }
}
