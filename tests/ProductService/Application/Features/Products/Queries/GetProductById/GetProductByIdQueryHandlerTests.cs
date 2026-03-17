using NSubstitute;
using ProductService.Application.Features.Products.DTOs;
using ProductService.Application.Features.Products.Queries.GetProductById;
using ProductService.Domain.Abstractions;
using ProductService.Domain.Entities;
using Xunit;

namespace ProductService.Tests.Application.Features.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ValidQuery_CallsRepositoryGetByIdAsync()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("Laptop", 15_000m, 10, "LAP-001");
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);
        var handler = new GetProductByIdQueryHandler(repository);
        var query = new GetProductByIdQuery(productId);

        await handler.Handle(query, CancellationToken.None);

        await repository.Received(1).GetByIdAsync(productId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsProductDto()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("Laptop", 15_000m, 10, "LAP-001");
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);
        var handler = new GetProductByIdQueryHandler(repository);
        var query = new GetProductByIdQuery(productId);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.IsType<ProductDto>(result);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsProductDtoWithCorrectProperties()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("Gaming Mouse", 500m, 50, "MOUSE-001");
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);
        var handler = new GetProductByIdQueryHandler(repository);
        var query = new GetProductByIdQuery(productId);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal("Gaming Mouse", result.Name);
        Assert.Equal(500m, result.Price);
        Assert.Equal(50, result.Stock);
        Assert.Equal("MOUSE-001", result.SKU);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ThrowsNotFoundException()
    {
        var productId = Guid.NewGuid();
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns((Product?)null);
        var handler = new GetProductByIdQueryHandler(repository);
        var query = new GetProductByIdQuery(productId);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(query, CancellationToken.None)
        );
    }
}
