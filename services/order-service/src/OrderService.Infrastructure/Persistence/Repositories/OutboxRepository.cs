using System.Threading;
using System.Threading.Tasks;
using OrderService.Application.Abstractions;
using OrderService.Infrastructure.Outbox;
using OrderService.Infrastructure.Persistence.Context;

namespace OrderService.Infrastructure.Persistence.Repositories
{
    public class OutboxRepository : IOutboxRepository
    {
        private readonly OrderDbContext _context;

        public OutboxRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(object domainEvent, CancellationToken cancellationToken = default)
        {
            var outboxMessage = new OutboxMessage(domainEvent);
            _context.Add(outboxMessage);
            await Task.CompletedTask;
        }
    }
}
