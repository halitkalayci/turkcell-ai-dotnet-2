using NSubstitute;
using OrderService.Application.Features.Orders.DTOs;
using OrderService.Application.Features.Orders.Queries.GetAllOrders;
using OrderService.Domain.Abstractions;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using Xunit;

namespace OrderService.Tests.Application.Features.Orders.Queries;

public class GetAllOrdersQueryHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly GetAllOrdersQueryHandler _handler;

    public GetAllOrdersQueryHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _handler = new GetAllOrdersQueryHandler(_orderRepository);
    }

    [Fact]
    public async Task Handle_OrdersExist_ReturnsOrderDtoList()
    {
        var customerId = Guid.NewGuid();
        var order = Order.Create(customerId);
        order.AddItem(Guid.NewGuid(), "Product 1", 100m, 2);
        var orders = new List<Order> { order };
        _orderRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(orders);
        var query = new GetAllOrdersQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(order.Id, result[0].Id);
        Assert.Equal(customerId, result[0].CustomerId);
        Assert.Single(result[0].OrderItems);
    }

    [Fact]
    public async Task Handle_NoOrders_ReturnsEmptyList()
    {
        _orderRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Order>());
        var query = new GetAllOrdersQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
