using MediatR;
using ProductService.Application.Features.Products.DTOs;
using ProductService.Domain.Abstractions;

namespace ProductService.Application.Features.Products.Queries.GetAllProducts;

public sealed class GetAllProductsQueryHandler
    : IRequestHandler<GetAllProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetAllProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IReadOnlyList<ProductDto>> Handle(
        GetAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetAllAsync(cancellationToken);
        return products.Select(p => p.ToDto()).ToList();
    }
}
