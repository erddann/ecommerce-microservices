namespace StockService.Infrastructure.Configuration
{
    public sealed class NotificationSettings
    {
        public bool EnableFailureAlerts { get; set; }
        public string Channel { get; set; } = "ops-alerts";
    }
}
