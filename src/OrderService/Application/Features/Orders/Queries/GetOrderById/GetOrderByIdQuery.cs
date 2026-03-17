using MediatR;
using OrderService.Application.Features.Orders.DTOs;

namespace OrderService.Application.Features.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto>;
