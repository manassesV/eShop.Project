using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBusRabbitMQ1
{
    public class EventBusOptions
    {
        public string SubscrriptionClientName { get; set; }

        public int RetryCount { get; set; }
    }
}
