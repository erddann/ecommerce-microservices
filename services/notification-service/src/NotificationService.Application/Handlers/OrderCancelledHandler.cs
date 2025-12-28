using Microsoft.Extensions.Logging;
using NotificationService.Application.Services;
using NotificationService.Infrastructure.Domain;
using NotificationService.Infrastructure.Domain.Events;

namespace NotificationService.Application.Handlers;

public interface IOrderCancelledHandler
{
    Task<bool> HandleAsync(EventEnvelope envelope, CancellationToken ct);
}

public class OrderCancelledHandler : IOrderCancelledHandler
{
	private readonly IOrderNotificationService _orderNotificationService;
    private readonly ILogger<OrderCancelledHandler> _logger;

    public OrderCancelledHandler(
		IOrderNotificationService orderNotificationService,
        ILogger<OrderCancelledHandler> logger)
    {
		_orderNotificationService = orderNotificationService;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(EventEnvelope envelope, CancellationToken ct)
    {
		_logger.LogInformation("OrderCancelled handler delegating event {EventId}", envelope.EventId);
		return await _orderNotificationService.ProcessOrderCancelledAsync(envelope, ct);
    }
}
