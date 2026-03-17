using MediatR;
using OrderService.Application.Abstractions;
using OrderService.Domain.Abstractions;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductServiceClient _productServiceClient;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IProductServiceClient productServiceClient)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _productServiceClient = productServiceClient;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var productStocks = await FetchAndValidateStocksAsync(request.Items, cancellationToken);

        var order = Order.Create(request.CustomerId);

        foreach (var item in request.Items)
        {
            var product = productStocks[item.ProductId];
            order.AddItem(item.ProductId, product.Name, product.Price, item.Quantity);
        }

        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Id;
    }

    private async Task<Dictionary<Guid, ProductStockDto>> FetchAndValidateStocksAsync(
        IReadOnlyList<OrderItemRequest> items,
        CancellationToken cancellationToken)
    {
        var productStocks = new Dictionary<Guid, ProductStockDto>();

        foreach (var item in items)
        {
            var product = await _productServiceClient.GetProductStockAsync(item.ProductId, cancellationToken)
                ?? throw new NotFoundException("Product", item.ProductId);

            if (product.Stock < item.Quantity)
                throw new InsufficientStockException(item.ProductId, item.Quantity, product.Stock);

            productStocks[item.ProductId] = product;
        }

        return productStocks;
    }
}
