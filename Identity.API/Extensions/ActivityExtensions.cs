using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Identity.API.Extensions
{
    public static class ActivityExtensions
    {
        public static void SetExceptionTags(this Activity activity, Exception ex)
        {
            if(activity is null)
            {
                return;
            }

            activity.AddTag("exception.message", ex.Message);
            activity.AddTag("exception.stacktrace", ex.ToString());
            activity.AddTag("exception.type", ex.GetType().FullName);
            activity.SetStatus(ActivityStatusCode.Error);
        }
    }
}
