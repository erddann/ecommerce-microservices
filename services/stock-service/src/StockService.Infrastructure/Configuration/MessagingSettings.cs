namespace StockService.Infrastructure.Configuration
{
    public sealed class MessagingSettings
    {
        public string ExchangeName { get; set; } = "order.events.topic";
        public string QueueName { get; set; } = "stock.ordercreated";
        public string OrderCreatedRoutingKey { get; set; } = "OrderCreatedEvent";
        public string DeadLetterExchange { get; set; } = "stock.dlq";
        public string DeadLetterQueue { get; set; } = "stock.ordercreated.dlq";
        public string DeadLetterRoutingKey { get; set; } = "stock.ordercreated.dlq";
        public int MaxRetryAttempts { get; set; } = 3;
        public ushort PrefetchCount { get; set; } = 1;
    }
}
