using NotificationService.Infrastructure.Domain;

namespace NotificationService.Infrastructure.Abstractions;

public interface INotificationTemplateRepository : IGenericRepository<NotificationTemplate>
{
    Task<NotificationTemplate?> GetAsync(string templateCode, NotificationChannel channel, string language, CancellationToken cancellationToken = default);
}
