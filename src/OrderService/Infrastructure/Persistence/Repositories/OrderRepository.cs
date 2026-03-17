using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Abstractions;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository : Repository<Order, Guid>, IOrderRepository
{
    public OrderRepository(OrderServiceDbContext context) : base(context)
    {
    }

    public async Task<Order?> GetOrderWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetOrdersByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.OrderItems)
            .Where(o => o.CustomerId == customerId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public override async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.OrderItems)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
