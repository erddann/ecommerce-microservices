using System.Threading;
using System.Threading.Tasks;
using OrderService.Infrastructure.Messaging;

namespace OrderService.Application.Handlers
{
    public interface IOrderStockFailMessageHandler
    {
        Task<bool> HandleAsync(EventEnvelope envelope, bool isFinalAttempt, CancellationToken cancellationToken);
    }
}
