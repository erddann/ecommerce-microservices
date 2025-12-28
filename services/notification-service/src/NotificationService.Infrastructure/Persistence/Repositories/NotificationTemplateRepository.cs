using Microsoft.EntityFrameworkCore;
using NotificationService.Infrastructure.Abstractions;
using NotificationService.Infrastructure.Domain;

namespace NotificationService.Infrastructure.Persistence.Repositories;

public class NotificationTemplateRepository : GenericRepository<NotificationTemplate>, INotificationTemplateRepository
{
    public NotificationTemplateRepository(NotificationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<NotificationTemplate?> GetAsync(string templateCode, NotificationChannel channel, string language, CancellationToken cancellationToken = default)
    {
        return await DbContext.NotificationTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TemplateCode == templateCode && x.Channel == channel && x.Language == language, cancellationToken);
    }
}
