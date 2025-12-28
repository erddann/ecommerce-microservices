using NotificationService.Infrastructure.Domain;

namespace NotificationService.Application.Services;

public class NotificationContext
{
    public string TemplateCode { get; set; } = string.Empty;
    public NotificationChannel Channel { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
}
