using MediatR;

namespace OrderService.Application.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(Guid CustomerId) : IRequest<Guid>;
