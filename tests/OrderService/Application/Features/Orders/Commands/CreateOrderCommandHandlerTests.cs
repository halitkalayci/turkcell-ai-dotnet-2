using MediatR;
using NSubstitute;
using OrderService.Application.Features.Orders.Commands.CreateOrder;
using OrderService.Domain.Abstractions;
using OrderService.Domain.Entities;
using Xunit;

namespace OrderService.Tests.Application.Features.Orders.Commands;

public class CreateOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreateOrderCommandHandler(_orderRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewOrderId()
    {
        var customerId = Guid.NewGuid();
        var command = new CreateOrderCommand(customerId);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
        await _orderRepository.Received(1).AddAsync(
            Arg.Is<Order>(o => o.CustomerId == customerId),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsUnitOfWorkSaveChanges()
    {
        var customerId = Guid.NewGuid();
        var command = new CreateOrderCommand(customerId);

        await _handler.Handle(command, CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
