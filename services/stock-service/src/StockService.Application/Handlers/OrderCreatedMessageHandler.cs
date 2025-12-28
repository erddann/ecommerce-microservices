using Microsoft.Extensions.Logging;
using StockService.Application.Contracts;
using StockService.Infrastructure.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace StockService.Application.Handlers
{
    public sealed class OrderCreatedMessageHandler : IOrderCreatedMessageHandler
    {
        private readonly IStockService _stockService;
        private readonly ILogger<OrderCreatedMessageHandler> _logger;

        public OrderCreatedMessageHandler(
            IStockService stockService,
            ILogger<OrderCreatedMessageHandler> logger)
        {
            _stockService = stockService;
            _logger = logger;
        }

        public async Task HandleAsync(EventEnvelope envelope, bool isFinalAttempt, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Dispatching order created event {EventId}", envelope.EventId);
            await _stockService.HandleOrderCreatedAsync(envelope, isFinalAttempt, cancellationToken);
        }
    }
}
