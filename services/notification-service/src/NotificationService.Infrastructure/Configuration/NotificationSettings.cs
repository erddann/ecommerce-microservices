namespace NotificationService.Infrastructure.Configuration;

public sealed class NotificationSettings
{
    public const string SectionName = "Notification";
    public ushort PrefetchCount { get; set; } = 1;
}
