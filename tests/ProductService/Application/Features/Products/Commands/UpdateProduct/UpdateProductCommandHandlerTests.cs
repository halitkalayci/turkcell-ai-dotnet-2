using NSubstitute;
using ProductService.Application.Features.Products.Commands.UpdateProduct;
using ProductService.Domain.Abstractions;
using ProductService.Domain.Entities;
using Xunit;

namespace ProductService.Tests.Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CallsRepositoryGetByIdAsync()
    {
        var productId = Guid.NewGuid();
        var existingProduct = Product.Create("Old Name", 100m, 5, "OLD-001");
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(existingProduct);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateProductCommandHandler(repository, unitOfWork);
        var command = new UpdateProductCommand(productId, "New Name", 200m, 10, "NEW-001");

        await handler.Handle(command, CancellationToken.None);

        await repository.Received(1).GetByIdAsync(productId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsRepositoryUpdate()
    {
        var productId = Guid.NewGuid();
        var existingProduct = Product.Create("Old Name", 100m, 5, "OLD-001");
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(existingProduct);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateProductCommandHandler(repository, unitOfWork);
        var command = new UpdateProductCommand(productId, "New Name", 200m, 10, "NEW-001");

        await handler.Handle(command, CancellationToken.None);

        repository.Received(1).Update(Arg.Is<Product>(p => p.Name == "New Name"));
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsUnitOfWorkSaveChangesAsync()
    {
        var productId = Guid.NewGuid();
        var existingProduct = Product.Create("Old Name", 100m, 5, "OLD-001");
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(existingProduct);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateProductCommandHandler(repository, unitOfWork);
        var command = new UpdateProductCommand(productId, "New Name", 200m, 10, "NEW-001");

        await handler.Handle(command, CancellationToken.None);

        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ProductNotFound_ThrowsNotFoundException()
    {
        var productId = Guid.NewGuid();
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns((Product?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateProductCommandHandler(repository, unitOfWork);
        var command = new UpdateProductCommand(productId, "New Name", 200m, 10, "NEW-001");

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesProductWithCorrectValues()
    {
        var productId = Guid.NewGuid();
        var existingProduct = Product.Create("Old Name", 100m, 5, "OLD-001");
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(existingProduct);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateProductCommandHandler(repository, unitOfWork);
        var command = new UpdateProductCommand(productId, "Updated Name", 300m, 20, "UPD-001");

        await handler.Handle(command, CancellationToken.None);

        repository.Received(1).Update(
            Arg.Is<Product>(p =>
                p.Name == "Updated Name" &&
                p.Price == 300m &&
                p.Stock == 20 &&
                p.SKU == "UPD-001"
            )
        );
    }
}
