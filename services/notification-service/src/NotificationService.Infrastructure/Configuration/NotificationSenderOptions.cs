namespace NotificationService.Infrastructure.Configuration;

public class NotificationSenderOptions
{
    public const string SectionName = "NotificationSender";
    public string Endpoint { get; set; } = "https://httpbin.org/post";
}
