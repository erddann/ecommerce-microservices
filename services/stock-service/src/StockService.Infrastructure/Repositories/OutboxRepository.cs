using StockService.Infrastructure.Abstractions;
using StockService.Infrastructure.Entities;
using StockService.Infrastructure.Persistence.Context;

namespace StockService.Infrastructure.Repositories
{
    public class OutboxRepository : IOutboxRepository
    {
        private readonly StockDbContext _db;

        public OutboxRepository(StockDbContext db)
        {
            _db = db;
        }

        public Task AddAsync(object domainEvent, CancellationToken cancellationToken)
        {
            var message = new OutboxMessage(domainEvent);
            _db.OutboxMessages.Add(message);
            return Task.CompletedTask;
        }
    }
}
