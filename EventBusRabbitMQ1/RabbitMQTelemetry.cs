using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTelemetry.Context.Propagation;

namespace EventBusRabbitMQ1
{
    public class RabbitMQTelemetry
    {
        public static string ActivitySourceName = "EventBusRabbitMQ";

        public ActivitySource activitySource { get; } = new(ActivitySourceName);
        public TextMapPropagator Propagator { get; } = Propagators.DefaultTextMapPropagator;
    }
}
