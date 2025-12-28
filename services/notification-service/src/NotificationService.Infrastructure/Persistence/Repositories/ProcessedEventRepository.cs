using Microsoft.EntityFrameworkCore;
using NotificationService.Infrastructure.Abstractions;
using NotificationService.Infrastructure.Domain;

namespace NotificationService.Infrastructure.Persistence.Repositories;

public class ProcessedEventRepository : IProcessedEventRepository
{
    private readonly NotificationDbContext _dbContext;

    public ProcessedEventRepository(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ExistsAsync(Guid eventId, string queueName, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProcessedEvents.AnyAsync(x => x.EventId == eventId && x.QueueName == queueName, cancellationToken);
    }

    public async Task AddAsync(ProcessedEvent processedEvent, CancellationToken cancellationToken = default)
    {
        await _dbContext.ProcessedEvents.AddAsync(processedEvent, cancellationToken);
    }

    public async Task MarkAsProcessedAsync(Guid eventId, string queueName, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ProcessedEvents.FirstOrDefaultAsync(x => x.EventId == eventId && x.QueueName == queueName, cancellationToken);
        if (entity != null)
        {
            entity.MarkAsProcessed();
        }
    }
}
