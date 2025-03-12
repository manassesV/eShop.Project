

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;
using eShop.EventBus.Events;

namespace eShop.IntegrationEventLogEF
{
    public class IntegrationEventLogEntry
    {
        private static readonly JsonSerializerOptions s_indentedOptions = new() { WriteIndented = true };
        private static readonly JsonSerializerOptions s_caseInsensitiveOptions = new() { PropertyNameCaseInsensitive = true };


        public IntegrationEventLogEntry(){ }

        public IntegrationEventLogEntry(IntegrationEvent @event, Guid transationId) {
            EventId = @event.Id;
            CreationTime = @event.CreationDate;
            EventTypeName = @event.GetType().FullName;
            Content = JsonSerializer.Serialize(@event, @event.GetType(), s_indentedOptions);
            State = EventStateEnum.NotPublished;
            TimesSent = 0;
            TransactionId = transationId;
        }

        public Guid EventId { get; private set; }
        [Required]
        public string EventTypeName { get; private set; }
        [NotMapped]
        public string EventTypeShortName => EventTypeName.Split('.')?.Last();
        [NotMapped]
        public IntegrationEvent IntegrationEvent { get; private set; }
        public EventStateEnum State { get;  set; }
        public int TimesSent { get; set; }
        public DateTime CreationTime { get; private set; }
        [Required]
        public string Content { get; private set; }
        public Guid TransactionId { get; private set; }

        public IntegrationEventLogEntry DeserializedJsonContent(Type type)
        {
            IntegrationEvent = JsonSerializer.Deserialize(Content, type, s_caseInsensitiveOptions) as IntegrationEvent;

            return this;
        }

    }
}
