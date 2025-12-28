using StockService.Infrastructure.Abstractions;
using StockService.Infrastructure.Persistence.Context;

namespace StockService.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly StockDbContext _db;

        public UnitOfWork(StockDbContext db)
        {
            _db = db;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _db.SaveChangesAsync(cancellationToken);
        }
    }
}
