using OrderService.Domain.Abstractions;

namespace OrderService.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly OrderServiceDbContext _context;

    public UnitOfWork(OrderServiceDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
