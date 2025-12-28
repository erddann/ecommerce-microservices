using NotificationService.Infrastructure.Domain.Events;
using NotificationService.Infrastructure.Domain;

namespace NotificationService.Application.Services;

public interface IOrderNotificationService
{
    Task<bool> ProcessOrderConfirmedAsync(EventEnvelope envelope, CancellationToken ct);
    Task<bool> ProcessOrderCancelledAsync(EventEnvelope envelope, CancellationToken ct);
}
