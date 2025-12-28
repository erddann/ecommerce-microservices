namespace OrderService.Infrastructure.Settings
{
    public sealed class OutboxSettings
    {
        public int PublisherBatchSize { get; set; } = 5;
    }
}
