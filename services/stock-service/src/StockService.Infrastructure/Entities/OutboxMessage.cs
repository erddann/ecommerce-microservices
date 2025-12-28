using StockService.Infrastructure.Messaging;
using System.Text.Json;

namespace StockService.Infrastructure.Entities
{
    public class OutboxMessage
    {
        private OutboxMessage() { }

        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid EventId { get; private set; }
        public DateTime OccurredOn { get; private set; }
        public string Type { get; private set; } = default!;
        public string Payload { get; private set; } = default!;
        public DateTime? ProcessedOn { get; private set; }

        public OutboxMessage(object domainEvent)
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;

            if (domainEvent is not StockService.Infrastructure.DomainFiles.DomainEvent de)
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

        private static T GetProperty<T>(object obj, string name)
        {
            var prop = obj.GetType().GetProperty(name);
            if (prop != null && prop.PropertyType == typeof(T))
            {
                return (T)prop.GetValue(obj)!;
            }

            throw new InvalidOperationException($"Domain event missing property {name} of type {typeof(T).Name}");
        }
    }
}
