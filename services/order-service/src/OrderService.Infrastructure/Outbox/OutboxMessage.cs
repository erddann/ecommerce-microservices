using OrderService.Infrastructure.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Outbox
{
    public class OutboxMessage
    {
        private OutboxMessage() { }
        public Guid Id { get; private set; }
        public Guid EventId { get; private set; }
        public DateTime OccurredOn { get; private set; }
        public string Type { get; private set; } = default!;
        public string Payload { get; private set; } = default!;
        public DateTime? ProcessedOn { get; private set; }
        public OutboxMessage(object domainEvent)
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;

            if (domainEvent is not OrderService.Domain.Common.DomainEvent de)
            {
                throw new ArgumentException("OutboxMessage expects a DomainEvent", nameof(domainEvent));
            }

            EventId = de.Id;
            Type = domainEvent.GetType().Name;

            var envelope = new EventEnvelope
            {
                EventId = EventId,
                EventType = Type,
                OccurredOn = de.OccurredOn,
                Data = JsonSerializer.SerializeToElement(domainEvent)
            };

            Payload = JsonSerializer.Serialize(envelope);
        }

        public void MarkAsProcessed()
        {
            ProcessedOn = DateTime.UtcNow;
        }
    }
}
