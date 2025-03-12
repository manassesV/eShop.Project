using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus1
{
    public interface IEventBusBuilder
    {
        public IServiceCollection Services { get; }
    }
}
