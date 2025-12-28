using NotificationService.Infrastructure.Abstractions;
using NotificationService.Infrastructure.Domain;

namespace NotificationService.Infrastructure.Persistence.Repositories;

public class OutboxRepository : IOutboxRepository
{
    private readonly NotificationDbContext _dbContext;

    public OutboxRepository(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await _dbContext.OutboxMessages.AddAsync(message, cancellationToken);
    }
}
