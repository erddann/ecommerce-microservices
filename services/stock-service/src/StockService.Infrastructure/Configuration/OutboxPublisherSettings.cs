namespace StockService.Infrastructure.Configuration
{
    public sealed class OutboxPublisherSettings
    {
        public string ExchangeName { get; set; } = "stock.events.topic";
        public int BatchSize { get; set; } = 50;
        public int PollIntervalSeconds { get; set; } = 5;
    }
}
