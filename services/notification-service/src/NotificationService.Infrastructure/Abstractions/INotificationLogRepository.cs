using NotificationService.Infrastructure.Domain;

namespace NotificationService.Infrastructure.Abstractions;

public interface INotificationLogRepository
{
    Task AddAsync(NotificationLog log, CancellationToken cancellationToken = default);
}
