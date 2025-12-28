using StockService.Infrastructure.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace StockService.Application.Handlers
{
    public interface IOrderCreatedMessageHandler
    {
        Task HandleAsync(EventEnvelope envelope, bool isFinalAttempt, CancellationToken cancellationToken);
    }
}
