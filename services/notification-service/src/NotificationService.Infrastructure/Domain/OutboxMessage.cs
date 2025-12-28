using System.Text.Json;

namespace NotificationService.Infrastructure.Domain;

public class OutboxMessage
{
    private OutboxMessage()
    {
    }

    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public JsonElement Data { get; private set; }
    public string Payload { get; private set; } = string.Empty;
    public DateTime? ProcessedOn { get; private set; }

    public static OutboxMessage FromDomainEvent<T>(string eventType, Guid eventId, DateTime occurredOn, T domainEvent)
    {
        var envelope = new EventEnvelope(eventId, eventType, occurredOn, JsonSerializer.SerializeToElement(domainEvent));
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            OccurredOn = occurredOn,
            Type = eventType,
            Data = envelope.Data,
            Payload = JsonSerializer.Serialize(envelope)
        };
    }

    public void MarkAsProcessed()
    {
        ProcessedOn = DateTime.UtcNow;
    }
}

public record EventEnvelope(Guid EventId, string EventType, DateTime OccurredOn, JsonElement Data);
