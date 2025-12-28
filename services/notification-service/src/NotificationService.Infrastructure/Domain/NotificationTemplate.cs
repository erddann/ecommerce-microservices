using NotificationService.Infrastructure.Domain;

namespace NotificationService.Infrastructure.Domain;

public class NotificationTemplate
{
    public Guid Id { get; private set; }
    public string TemplateCode { get; private set; } = string.Empty;
    public NotificationChannel Channel { get; private set; }
    public string Language { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public int Version { get; private set; }

    private NotificationTemplate()
    {
    }

    public NotificationTemplate(string templateCode, NotificationChannel channel, string language, string subject, string body, bool isActive, int version)
    {
        Id = Guid.NewGuid();
        TemplateCode = templateCode;
        Channel = channel;
        Language = language;
        Subject = subject;
        Body = body;
        IsActive = isActive;
        Version = version;
    }
}
