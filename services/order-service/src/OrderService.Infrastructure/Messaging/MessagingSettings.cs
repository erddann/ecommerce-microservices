namespace OrderService.Infrastructure.Messaging
{
    public sealed class MessagingSettings
    {
        public string OutgoingExchange { get; set; } = "order.events.topic";

        public IncomingStockSettings IncomingStock { get; set; } = new();
    }

    public sealed class IncomingStockSettings
    {
        public string Exchange { get; set; } = "stock.events.topic";
        public string FailQueue { get; set; } = "order.stock.fail";
        public string CompletedQueue { get; set; } = "order.stock.completed";
        public string FailRoutingKey { get; set; } = "OrderStockProcessCompletedDomainEvent";
        public string CompletedRoutingKey { get; set; } = "OrderStockProcessFailedDomainEvent";
    }
}
