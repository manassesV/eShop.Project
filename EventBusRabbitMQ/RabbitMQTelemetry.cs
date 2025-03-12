using System.Diagnostics;
using OpenTelemetry.Context.Propagation;

namespace eShop.EventBusRabbitMQ
{
    public class RabbitMQTelemetry
    {
        public static string ActivitySourceName = "EventBusRabbitMQ";

        public ActivitySource ActivitySource { get; set; } = new(ActivitySourceName);
        public TextMapPropagator Propagator { get; } = Propagators.DefaultTextMapPropagator;
    }
}
