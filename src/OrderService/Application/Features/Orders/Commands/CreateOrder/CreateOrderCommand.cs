using MediatR;

namespace OrderService.Application.Features.Orders.Commands.CreateOrder;

public sealed record OrderItemRequest(Guid ProductId, int Quantity);

public sealed record CreateOrderCommand(Guid CustomerId, IReadOnlyList<OrderItemRequest> Items) : IRequest<Guid>;
