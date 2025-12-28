using Microsoft.Extensions.Logging;
using NotificationService.Application.Services;
using NotificationService.Infrastructure.Domain;
using NotificationService.Infrastructure.Domain.Events;

namespace NotificationService.Application.Handlers;

public interface IOrderConfirmedHandler
{
    Task<bool> HandleAsync(EventEnvelope envelope, CancellationToken ct);
}

public class OrderConfirmedHandler : IOrderConfirmedHandler
{
	private readonly IOrderNotificationService _orderNotificationService;
    private readonly ILogger<OrderConfirmedHandler> _logger;

    public OrderConfirmedHandler(
		IOrderNotificationService orderNotificationService,
        ILogger<OrderConfirmedHandler> logger)
    {
		_orderNotificationService = orderNotificationService;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(EventEnvelope envelope, CancellationToken ct)
    {
		_logger.LogInformation("OrderConfirmed handler delegating event {EventId}", envelope.EventId);
		return await _orderNotificationService.ProcessOrderConfirmedAsync(envelope, ct);
    }
}
