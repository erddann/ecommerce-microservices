namespace NotificationService.Infrastructure.Abstractions;

public interface IOrderEventProcessor
{
    Task HandleOrderCancelledAsync(string payload, CancellationToken cancellationToken);
    Task HandleOrderConfirmedAsync(string payload, CancellationToken cancellationToken);
}
