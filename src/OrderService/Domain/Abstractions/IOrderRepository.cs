using OrderService.Domain.Entities;

namespace OrderService.Domain.Abstractions;

public interface IOrderRepository : IRepository<Order, Guid>
{
    Task<Order?> GetOrderWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetOrdersByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
}
