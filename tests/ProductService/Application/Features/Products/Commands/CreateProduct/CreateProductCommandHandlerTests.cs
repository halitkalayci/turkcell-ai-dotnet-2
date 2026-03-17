using NSubstitute;
using ProductService.Application.Features.Products.Commands.CreateProduct;
using ProductService.Application.Features.Products.DTOs;
using ProductService.Domain.Abstractions;
using ProductService.Domain.Entities;
using Xunit;

namespace ProductService.Tests.Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ReturnsProductDto()
    {
        var repository = Substitute.For<IProductRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateProductCommandHandler(repository, unitOfWork);
        var command = new CreateProductCommand("Laptop", 15_000m, 10, "LAP-001");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.IsType<ProductDto>(result);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsRepositoryAddAsync()
    {
        var repository = Substitute.For<IProductRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateProductCommandHandler(repository, unitOfWork);
        var command = new CreateProductCommand("Laptop", 15_000m, 10, "LAP-001");

        await handler.Handle(command, CancellationToken.None);

        await repository.Received(1).AddAsync(
            Arg.Is<Product>(p => p.Name == "Laptop" && p.Price == 15_000m && p.Stock == 10 && p.SKU == "LAP-001"),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsUnitOfWorkSaveChangesAsync()
    {
        var repository = Substitute.For<IProductRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateProductCommandHandler(repository, unitOfWork);
        var command = new CreateProductCommand("Laptop", 15_000m, 10, "LAP-001");

        await handler.Handle(command, CancellationToken.None);

        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsProductDtoWithCorrectProperties()
    {
        var repository = Substitute.For<IProductRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateProductCommandHandler(repository, unitOfWork);
        var command = new CreateProductCommand("Gaming Laptop", 25_000m, 5, "GAME-001");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("Gaming Laptop", result.Name);
        Assert.Equal(25_000m, result.Price);
        Assert.Equal(5, result.Stock);
        Assert.Equal("GAME-001", result.SKU);
        Assert.NotEqual(Guid.Empty, result.Id);
    }
}
