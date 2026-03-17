using MediatR;
using ProductService.Application.Features.Products.DTOs;

namespace ProductService.Application.Features.Products.Queries.GetAllProducts;

public sealed record GetAllProductsQuery : IRequest<IReadOnlyList<ProductDto>>;
