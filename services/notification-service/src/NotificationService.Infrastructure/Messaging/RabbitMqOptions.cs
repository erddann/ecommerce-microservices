namespace NotificationService.Infrastructure.Messaging;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string ExchangeName { get; set; } = "order.events.topic";
    public string OrderCancelledQueue { get; set; } = "order.cancelled.queue";
    public string OrderCancelledRoutingKey { get; set; } = "OrderCancelledEvent";
    public string OrderConfirmedQueue { get; set; } = "order.confirmed.queue";
    public string OrderConfirmedRoutingKey { get; set; } = "OrderConfirmedEvent";
}
