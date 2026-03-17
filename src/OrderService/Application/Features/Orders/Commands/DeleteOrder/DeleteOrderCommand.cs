using MediatR;

namespace OrderService.Application.Features.Orders.Commands.DeleteOrder;

public sealed record DeleteOrderCommand(Guid OrderId) : IRequest<Unit>;
