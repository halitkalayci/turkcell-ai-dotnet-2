using MediatR;
using ProductService.Application.Features.Products.DTOs;

namespace ProductService.Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    decimal Price,
    int Stock,
    string SKU
) : IRequest<ProductDto>;
