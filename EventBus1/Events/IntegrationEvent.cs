using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EventBus1.Events
{
    public record IntegrationEvent
    {
        public IntegrationEvent()
        {
             Id = Guid.NewGuid();
            CreationDate = DateTime.Now;
        }

        [JsonInclude]
        public Guid Id { get; set; }

        [JsonInclude]
        public DateTime CreationDate { get; set; }
    }
}
