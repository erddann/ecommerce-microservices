using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Infrastructure.Abstractions;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
    protected NotificationDbContext DbContext { get; }
    protected DbSet<TEntity> Entities { get; }

    protected GenericRepository(NotificationDbContext dbContext)
    {
        DbContext = dbContext;
        Entities = dbContext.Set<TEntity>();
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Entities.AsNoTracking().ToListAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Entities.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Entities.AddAsync(entity, cancellationToken);
    }

    public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Entities.Update(entity);
        return Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            return;
        }

        Entities.Remove(entity);
    }
}
