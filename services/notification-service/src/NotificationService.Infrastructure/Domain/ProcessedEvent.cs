namespace NotificationService.Infrastructure.Domain;

public class ProcessedEvent
{
    private ProcessedEvent()
    {
    }

    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string QueueName { get; private set; } = string.Empty;
    public Guid EventId { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public DateTime? ProcessedOn { get; private set; }

    public static ProcessedEvent Create(string eventType, string queueName, Guid eventId, DateTime occurredOn)
    {
        return new ProcessedEvent
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            QueueName = queueName,
            EventId = eventId,
            OccurredOn = occurredOn,
            ProcessedOn = null
        };
    }

    public void MarkAsProcessed()
    {
        ProcessedOn = DateTime.UtcNow;
    }
}
