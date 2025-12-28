using NotificationService.Application.Contracts;
using NotificationService.Infrastructure.Domain;

namespace NotificationService.Application.Services;

public interface INotificationTemplateService
{
    Task<IReadOnlyList<NotificationTemplate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<NotificationTemplate> CreateAsync(CreateNotificationTemplateRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
