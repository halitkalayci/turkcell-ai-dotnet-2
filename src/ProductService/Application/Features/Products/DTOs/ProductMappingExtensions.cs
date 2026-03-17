using ProductService.Domain.Entities;

namespace ProductService.Application.Features.Products.DTOs;

public static class ProductMappingExtensions
{
    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto(
            product.Id,
            product.Name,
            product.Price,
            product.Stock,
            product.SKU,
            product.CreatedAt,
            product.UpdatedAt
        );
    }
}
