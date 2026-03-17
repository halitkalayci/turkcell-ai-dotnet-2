using MediatR;
using NSubstitute;
using OrderService.Application.Features.Orders.Commands.DeleteOrder;
using OrderService.Domain.Abstractions;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;
using Xunit;

namespace OrderService.Tests.Application.Features.Orders.Commands;

public class DeleteOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteOrderCommandHandler _handler;

    public DeleteOrderCommandHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new DeleteOrderCommandHandler(_orderRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_SoftDeletesOrder()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var order = Order.Create(customerId);
        _orderRepository.GetByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns(order);
        var command = new DeleteOrderCommand(orderId);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(Unit.Value, result);
        Assert.True(order.IsDeleted);
        _orderRepository.Received(1).Update(order);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ThrowsNotFoundException()
    {
        var orderId = Guid.NewGuid();
        _orderRepository.GetByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns((Order?)null);
        var command = new DeleteOrderCommand(orderId);

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
        _orderRepository.GetByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns(order);
        var command = new DeleteOrderCommand(orderId);

        await _handler.Handle(command, CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
