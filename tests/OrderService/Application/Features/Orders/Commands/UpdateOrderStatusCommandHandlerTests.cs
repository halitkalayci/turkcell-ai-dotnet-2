using MediatR;
using NSubstitute;
using OrderService.Application.Features.Orders.Commands.UpdateOrderStatus;
using OrderService.Domain.Abstractions;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Exceptions;
using Xunit;

namespace OrderService.Tests.Application.Features.Orders.Commands;

public class UpdateOrderStatusCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateOrderStatusCommandHandler _handler;

    public UpdateOrderStatusCommandHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new UpdateOrderStatusCommandHandler(_orderRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommandWithConfirmedStatus_UpdatesOrderStatus()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var order = Order.Create(customerId);
        order.AddItem(Guid.NewGuid(), "Product 1", 100m, 2);
        _orderRepository.GetByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns(order);
        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Confirmed);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(Unit.Value, result);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
        _orderRepository.Received(1).Update(order);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ThrowsNotFoundException()
    {
        var orderId = Guid.NewGuid();
        _orderRepository.GetByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns((Order?)null);
        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Confirmed);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsUnitOfWorkSaveChanges()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var order = Order.Create(customerId);
        order.AddItem(Guid.NewGuid(), "Product 1", 100m, 2);
        _orderRepository.GetByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns(order);
        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Confirmed);

        await _handler.Handle(command, CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
