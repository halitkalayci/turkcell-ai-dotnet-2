using MediatR;
using OrderService.Domain.Enums;

namespace OrderService.Application.Features.Orders.Commands.UpdateOrderStatus;

public sealed record UpdateOrderStatusCommand(Guid OrderId, OrderStatus NewStatus) : IRequest<Unit>;
