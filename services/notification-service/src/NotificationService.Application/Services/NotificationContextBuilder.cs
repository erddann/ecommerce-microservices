using System.Threading;
using System.Threading.Tasks;
using NotificationService.Infrastructure.Abstractions;
using NotificationService.Infrastructure.Domain;
using NotificationService.Infrastructure.Domain.Events;

namespace NotificationService.Application.Services;

public class NotificationContextBuilder
{
    private readonly ICustomerServiceClient _customerServiceClient;

    public NotificationContextBuilder(ICustomerServiceClient customerServiceClient)
    {
        _customerServiceClient = customerServiceClient;
    }

    public async Task<NotificationContext> BuildContextAsync(OrderConfirmedEvent @event, CancellationToken cancellationToken)
    {
        var customer = await _customerServiceClient.GetCustomerAsync(@event.CustomerId, cancellationToken);
        var recipientEmail = customer.Email;
        var customerName = customer.FullName;

        return new NotificationContext
        {
            TemplateCode = "ORDER_CONFIRMED",
            Channel = NotificationChannel.Email,
            CustomerId = @event.CustomerId,
            CustomerName = customerName,
            RecipientEmail = recipientEmail,
            Data = new Dictionary<string, object>
            {
                ["OrderNumber"] = @event.OrderId.ToString(),
                ["OrderTotal"] = @event.OrderTotal,
                ["CustomerName"] = customerName
            }
        };
    }

    public async Task<NotificationContext> BuildContextAsync(OrderCancelledEvent @event, CancellationToken cancellationToken)
    {
        var customer = await _customerServiceClient.GetCustomerAsync(@event.CustomerId, cancellationToken);

        return new NotificationContext
        {
            TemplateCode = "ORDER_CANCELLED",
            Channel = NotificationChannel.Email,
            CustomerId = @event.CustomerId,
            CustomerName = customer.FullName,
            RecipientEmail = customer.Email,
            Data = new Dictionary<string, object>
            {
                ["OrderId"] = @event.OrderId.ToString(),
                ["Reason"] = @event.Reason,
                ["CustomerName"] = customer.FullName
            }
        };
    }
}
