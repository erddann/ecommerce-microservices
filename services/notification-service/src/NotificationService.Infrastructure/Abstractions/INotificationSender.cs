using NotificationService.Infrastructure.Domain;

namespace NotificationService.Infrastructure.Abstractions;

public interface INotificationSender
{
    Task<NotificationSendResult> SendAsync(RenderedNotification notification, CancellationToken cancellationToken = default);
}

public record NotificationSendResult(bool Success, string Status, string Destination, string? Error = null);

public record RenderedNotification(string Subject, string Body, NotificationChannel Channel, string Destination);

public interface IChannelSender
{
    NotificationChannel Channel { get; }
    Task<NotificationSendResult> SendAsync(RenderedNotification notification, CancellationToken cancellationToken = default);
}
