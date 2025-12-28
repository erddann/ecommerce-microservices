using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Abstractions;
using OrderService.Infrastructure.Entities;
using OrderService.Infrastructure.Persistence.Context;

namespace OrderService.Infrastructure.Persistence.Repositories
{
    public class ProcessedEventRepository : IProcessedEventRepository
    {
        private readonly OrderDbContext _context;

        public ProcessedEventRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(string eventType, Guid eventId, CancellationToken cancellationToken = default)
        {
            return await _context.ProcessedEvents.AnyAsync(p => p.EventType == eventType && p.EventId == eventId, cancellationToken);
        }

        public async Task AddAsync(string eventType, Guid eventId, string queueName, CancellationToken cancellationToken = default)
        {
            var processed = ProcessedEvent.Create(eventType, queueName, eventId);
            _context.Add(processed);
            await Task.CompletedTask;
        }

        public void MarkAsProcessed(string eventType, Guid eventId)
        {
            // ProcessedEvent entity already tracked
            // In this simplified implementation nothing extra needed beyond SaveChanges
        }
    }
}
