using OrderService.Domain.Enums;

namespace OrderService.Application.Features.Orders.DTOs;

public sealed record OrderDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public OrderStatus Status { get; init; }
    public DateTime OrderDate { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public IReadOnlyList<OrderItemDto> OrderItems { get; init; } = Array.Empty<OrderItemDto>();
}
