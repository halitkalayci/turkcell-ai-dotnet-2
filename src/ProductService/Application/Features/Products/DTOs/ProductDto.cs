namespace ProductService.Application.Features.Products.DTOs;

public sealed record ProductDto(
    Guid Id,
    string Name,
    decimal Price,
    int Stock,
    string SKU,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
