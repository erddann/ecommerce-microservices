using NotificationService.Infrastructure.Domain;

namespace NotificationService.Infrastructure.Abstractions;

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);
}
