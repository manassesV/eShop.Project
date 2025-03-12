using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace EventBusRabbitMQ1
{
    public static class ActivityExtension
    {
        public static void SetExceptionTags(this Activity activity, Exception ex)
        {
            if (activity is null)
                return;

            activity.AddTag("exception.message", ex.Message);
            activity.AddTag("exception.stacktrace", ex.ToString());
            activity.AddTag("exception.type", ex.GetType().FullName);
            activity.SetStatus(ActivityStatusCode.Error);
        }

    }
    


}
