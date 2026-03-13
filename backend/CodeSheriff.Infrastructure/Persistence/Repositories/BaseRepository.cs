using CodeSheriff.Domain.Common;
using CodeSheriff.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CodeSheriff.Infrastructure.Persistence.Repositories;

internal abstract class BaseRepository<T> : IRepository<T> where T : Entity
{
    protected readonly CodeSheriffDbContext Context;
    protected readonly DbSet<T> DbSet;

    protected BaseRepository(CodeSheriffDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet.FindAsync(new object[] { id }, cancellationToken);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().ToListAsync(cancellationToken);

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await DbSet.AddAsync(entity, cancellationToken);

    public virtual void Update(T entity)
        => DbSet.Update(entity);

    public virtual void Remove(T entity)
        => DbSet.Remove(entity);
}
