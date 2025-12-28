using MediatR;
using OrderService.Infrastructure.Persistence.Context;
using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Application.Behaviors
{
    public class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly OrderDbContext _context;

        public UnitOfWorkBehavior(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var response = await next();
            await _context.SaveChangesAsync(cancellationToken);
            return response;
        }
    }
}
