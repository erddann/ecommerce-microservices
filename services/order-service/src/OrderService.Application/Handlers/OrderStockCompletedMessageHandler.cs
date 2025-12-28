using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrderService.Application.Contracts;
using OrderService.Infrastructure.DomainFiles.Events;
using OrderService.Infrastructure.Messaging;

namespace OrderService.Application.Handlers
{
    public class OrderStockCompletedMessageHandler : IOrderStockCompletedMessageHandler
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderStockCompletedMessageHandler> _logger;

        public OrderStockCompletedMessageHandler(
            IOrderService orderService,
            ILogger<OrderStockCompletedMessageHandler> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        public async Task<bool> HandleAsync(EventEnvelope envelope, bool isFinalAttempt, CancellationToken cancellationToken)
        {
            var domainEvent = envelope.Data.Deserialize<OrderStockProcessCompletedDomainEvent>();
            if (domainEvent == null)
            {
                _logger.LogWarning("OrderStockCompleted handler failed to deserialize event for envelope {EventId}", envelope.EventId);
                return false;
            }

            try
            {
                await _orderService.HandleStockCompletedAsync(domainEvent, cancellationToken);
                return true;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Invalid payload for OrderStockCompleted event {EventId}", envelope.EventId);
                throw;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error processing OrderStockCompleted event {EventId}; FinalAttempt: {IsFinalAttempt}", envelope.EventId, isFinalAttempt);
                throw;
            }
        }
    }
}
