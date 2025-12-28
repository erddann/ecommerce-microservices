using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrderService.Application.Contracts;
using OrderService.Infrastructure.DomainFiles.Events;
using OrderService.Infrastructure.Messaging;

namespace OrderService.Application.Handlers
{
    public class OrderStockFailMessageHandler : IOrderStockFailMessageHandler
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderStockFailMessageHandler> _logger;

        public OrderStockFailMessageHandler(
            IOrderService orderService,
            ILogger<OrderStockFailMessageHandler> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        public async Task<bool> HandleAsync(EventEnvelope envelope, bool isFinalAttempt, CancellationToken cancellationToken)
        {
            var domainEvent = envelope.Data.Deserialize<OrderStockProcessFailedDomainEvent>();
            if (domainEvent == null)
            {
                _logger.LogWarning("OrderStockFail handler failed to deserialize event for envelope {EventId}", envelope.EventId);
                return false;
            }

            try
            {
                await _orderService.HandleStockFailedAsync(domainEvent, cancellationToken);
                return true;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Invalid payload for OrderStockFail event {EventId}", envelope.EventId);
                throw;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error processing OrderStockFail event {EventId}; FinalAttempt: {IsFinalAttempt}", envelope.EventId, isFinalAttempt);
                throw;
            }
        }
    }
}
