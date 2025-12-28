using NotificationService.Infrastructure.Domain;

namespace NotificationService.Infrastructure.Abstractions;

public interface IProcessedEventRepository
{
    Task<bool> ExistsAsync(Guid eventId, string queueName, CancellationToken cancellationToken = default);
    Task AddAsync(ProcessedEvent processedEvent, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(Guid eventId, string queueName, CancellationToken cancellationToken = default);
}
