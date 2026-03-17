using MediatR;

namespace ProductService.Application.Features.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    decimal Price,
    int Stock,
    string SKU
) : IRequest;
