using NotificationService.Infrastructure.Abstractions;
using NotificationService.Infrastructure.Domain;

namespace NotificationService.Infrastructure.Persistence.Repositories;

public class NotificationLogRepository : INotificationLogRepository
{
    private readonly NotificationDbContext _dbContext;

    public NotificationLogRepository(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(NotificationLog log, CancellationToken cancellationToken = default)
    {
        await _dbContext.NotificationLogs.AddAsync(log, cancellationToken);
    }
}
