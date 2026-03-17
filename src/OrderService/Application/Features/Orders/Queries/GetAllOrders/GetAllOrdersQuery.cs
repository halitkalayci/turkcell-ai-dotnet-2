using MediatR;
using OrderService.Application.Features.Orders.DTOs;

namespace OrderService.Application.Features.Orders.Queries.GetAllOrders;

public sealed record GetAllOrdersQuery : IRequest<IReadOnlyList<OrderDto>>;
