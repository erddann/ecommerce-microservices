using NotificationService.Infrastructure.Abstractions;

namespace NotificationService.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly NotificationDbContext _dbContext;

    public UnitOfWork(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
