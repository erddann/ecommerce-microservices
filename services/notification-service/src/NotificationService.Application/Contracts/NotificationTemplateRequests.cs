using NotificationService.Infrastructure.Domain;

namespace NotificationService.Application.Contracts;

public record CreateNotificationTemplateRequest(
    string TemplateCode,
    NotificationChannel Channel,
    string Language,
    string Subject,
    string Body,
    bool IsActive,
    int Version);

public record UpdateNotificationTemplateRequest(
    string TemplateCode,
    NotificationChannel Channel,
    string Language,
    string Subject,
    string Body,
    bool IsActive,
    int Version);
