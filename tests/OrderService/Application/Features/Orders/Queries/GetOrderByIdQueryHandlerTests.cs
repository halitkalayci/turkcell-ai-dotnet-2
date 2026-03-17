using NSubstitute;
using OrderService.Application.Features.Orders.DTOs;
using OrderService.Application.Features.Orders.Queries.GetOrderById;
using OrderService.Domain.Abstractions;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Exceptions;
using Xunit;

namespace OrderService.Tests.Application.Features.Orders.Queries;

public class GetOrderByIdQueryHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly GetOrderByIdQueryHandler _handler;

    public GetOrderByIdQueryHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _handler = new GetOrderByIdQueryHandler(_orderRepository);
    }

    [Fact]
    public async Task Handle_OrderExists_ReturnsOrderDto()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var order = Order.Create(customerId);
        order.AddItem(Guid.NewGuid(), "Product 1", 100m, 2);
        _orderRepository.GetOrderWithItemsAsync(orderId, Arg.Any<CancellationToken>())
            .Returns(order);
        var query = new GetOrderByIdQuery(orderId);

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(order.Id, result.Id);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Single(result.OrderItems);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ThrowsNotFoundException()
    {
        var orderId = Guid.NewGuid();
        _orderRepository.GetOrderWithItemsAsync(orderId, Arg.Any<CancellationToken>())
            .Returns((Order?)null);
        var query = new GetOrderByIdQuery(orderId);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(query, CancellationToken.None)
        );
    }
}
