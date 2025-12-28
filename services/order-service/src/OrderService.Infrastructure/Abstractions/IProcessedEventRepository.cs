using System;
using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Application.Abstractions
{
    public interface IProcessedEventRepository
    {
        Task<bool> ExistsAsync(string eventType, Guid eventId, CancellationToken cancellationToken = default);
        Task AddAsync(string eventType, Guid eventId, string queueName, CancellationToken cancellationToken = default);
        void MarkAsProcessed(string eventType, Guid eventId);
    }
}
