using NotificationService.Application.Contracts;
using NotificationService.Infrastructure.Abstractions;
using NotificationService.Infrastructure.Domain;

namespace NotificationService.Application.Services;

public class NotificationTemplateService : INotificationTemplateService
{
    private readonly INotificationTemplateRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationTemplateService(INotificationTemplateRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<NotificationTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return items.ToList();
    }

    public Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _repository.GetByIdAsync(id, cancellationToken);

    public async Task<NotificationTemplate> CreateAsync(CreateNotificationTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var template = new NotificationTemplate(
            request.TemplateCode,
            request.Channel,
            request.Language,
            request.Subject,
            request.Body,
            request.IsActive,
            request.Version);

        await _repository.AddAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return template;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            return false;
        }

        await _repository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
