using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Abstractions;

namespace ProductService.Infrastructure.Persistence.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity<Guid>
{
    protected readonly ProductServiceDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(ProductServiceDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(T entity)
    {
        DbSet.Update(entity);
    }

    public void Remove(T entity)
    {
        DbSet.Remove(entity);
    }
}
