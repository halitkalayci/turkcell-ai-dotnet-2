namespace OrderService.Application.Abstractions;

public interface IProductServiceClient
{
    Task<ProductStockDto?> GetProductStockAsync(Guid productId, CancellationToken cancellationToken = default);
}
