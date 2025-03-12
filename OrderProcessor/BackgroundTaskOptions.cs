using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessor
{
    public class BackgroundTaskOptions
    {
        public int GracePeriodTime {  get; set; }

        public int CheckUpdateTime { get; set; }
    }
}
