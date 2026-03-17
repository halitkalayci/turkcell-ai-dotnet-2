using NSubstitute;
using ProductService.Application.Features.Products.DTOs;
using ProductService.Application.Features.Products.Queries.GetAllProducts;
using ProductService.Domain.Abstractions;
using ProductService.Domain.Entities;
using Xunit;

namespace ProductService.Tests.Application.Features.Products.Queries.GetAllProducts;

public sealed class GetAllProductsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ValidQuery_CallsRepositoryGetAllAsync()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Product>());
        var handler = new GetAllProductsQueryHandler(repository);
        var query = new GetAllProductsQuery();

        await handler.Handle(query, CancellationToken.None);

        await repository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsProductDtoList()
    {
        var products = new List<Product>
        {
            Product.Create("Laptop", 15_000m, 10, "LAP-001"),
            Product.Create("Mouse", 500m, 50, "MOUSE-001")
        };
        var repository = Substitute.For<IProductRepository>();
        repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(products);
        var handler = new GetAllProductsQueryHandler(repository);
        var query = new GetAllProductsQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.IsAssignableFrom<IReadOnlyList<ProductDto>>(result);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsCorrectNumberOfProducts()
    {
        var products = new List<Product>
        {
            Product.Create("Laptop", 15_000m, 10, "LAP-001"),
            Product.Create("Mouse", 500m, 50, "MOUSE-001"),
            Product.Create("Keyboard", 1_200m, 30, "KEY-001")
        };
        var repository = Substitute.For<IProductRepository>();
        repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(products);
        var handler = new GetAllProductsQueryHandler(repository);
        var query = new GetAllProductsQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsProductsWithCorrectProperties()
    {
        var products = new List<Product>
        {
            Product.Create("Laptop", 15_000m, 10, "LAP-001"),
            Product.Create("Mouse", 500m, 50, "MOUSE-001")
        };
        var repository = Substitute.For<IProductRepository>();
        repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(products);
        var handler = new GetAllProductsQueryHandler(repository);
        var query = new GetAllProductsQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Contains(result, dto => dto.Name == "Laptop" && dto.Price == 15_000m);
        Assert.Contains(result, dto => dto.Name == "Mouse" && dto.Price == 500m);
    }

    [Fact]
    public async Task Handle_EmptyRepository_ReturnsEmptyList()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Product>());
        var handler = new GetAllProductsQueryHandler(repository);
        var query = new GetAllProductsQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
