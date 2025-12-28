namespace NotificationService.Infrastructure.Domain;

public class NotificationLog
{
    private NotificationLog()
    {
    }

    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public NotificationChannel Channel { get; private set; }
    public string Destination { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTime SentOn { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public string? Error { get; private set; }

    public static NotificationLog Create(Guid eventId, string eventType, NotificationChannel channel, string destination, string payload, string status, string? error = null)
    {
        return new NotificationLog
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            EventType = eventType,
            Channel = channel,
            Destination = destination,
            Payload = payload,
            SentOn = DateTime.UtcNow,
            Status = status,
            Error = string.IsNullOrWhiteSpace(error) ? null : error
        };
    }
}
