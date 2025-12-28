using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Infrastructure.Abstractions;
using NotificationService.Infrastructure.Configuration;

namespace NotificationService.Infrastructure.Services;

public class NotificationSender : INotificationSender
{
    private readonly IEnumerable<IChannelSender> _channelSenders;
    private readonly ILogger<NotificationSender> _logger;

    public NotificationSender(IEnumerable<IChannelSender> channelSenders, ILogger<NotificationSender> logger)
    {
        _channelSenders = channelSenders;
        _logger = logger;
    }

    public async Task<NotificationSendResult> SendAsync(RenderedNotification notification, CancellationToken cancellationToken = default)
    {
        var sender = _channelSenders.FirstOrDefault(s => s.Channel == notification.Channel);
        if (sender == null)
        {
            _logger.LogError("No sender found for channel {Channel}", notification.Channel);
            return new NotificationSendResult(false, "NoSender", $"No sender for channel {notification.Channel}");
        }
        try
        {
            _logger.LogInformation("Dispatching notification for channel {Channel}", notification.Channel);
            return await sender.SendAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error while sending notification for channel {Channel}", notification.Channel);
            throw;
        }
    }
}
