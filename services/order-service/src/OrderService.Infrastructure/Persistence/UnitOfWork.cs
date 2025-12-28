using System.Threading;
using System.Threading.Tasks;
using OrderService.Application.Abstractions;
using OrderService.Infrastructure.Persistence.Context;

namespace OrderService.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly OrderDbContext _context;

        public UnitOfWork(OrderDbContext context)
        {
            _context = context;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
