using NSubstitute;
using ProductService.Application.Features.Products.Commands.DeleteProduct;
using ProductService.Domain.Abstractions;
using ProductService.Domain.Entities;
using Xunit;

namespace ProductService.Tests.Application.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CallsRepositoryGetByIdAsync()
    {
        var productId = Guid.NewGuid();
        var existingProduct = Product.Create("Product Name", 100m, 10, "PROD-001");
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(existingProduct);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeleteProductCommandHandler(repository, unitOfWork);
        var command = new DeleteProductCommand(productId);

        await handler.Handle(command, CancellationToken.None);

        await repository.Received(1).GetByIdAsync(productId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsRepositoryRemove()
    {
        var productId = Guid.NewGuid();
        var existingProduct = Product.Create("Product Name", 100m, 10, "PROD-001");
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(existingProduct);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeleteProductCommandHandler(repository, unitOfWork);
        var command = new DeleteProductCommand(productId);

        await handler.Handle(command, CancellationToken.None);

        repository.Received(1).Remove(Arg.Is<Product>(p => p == existingProduct));
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsUnitOfWorkSaveChangesAsync()
    {
        var productId = Guid.NewGuid();
        var existingProduct = Product.Create("Product Name", 100m, 10, "PROD-001");
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(existingProduct);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeleteProductCommandHandler(repository, unitOfWork);
        var command = new DeleteProductCommand(productId);

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
        var handler = new DeleteProductCommandHandler(repository, unitOfWork);
        var command = new DeleteProductCommand(productId);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_ProductNotFound_DoesNotCallRemove()
    {
        var productId = Guid.NewGuid();
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns((Product?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeleteProductCommandHandler(repository, unitOfWork);
        var command = new DeleteProductCommand(productId);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None)
        );

        repository.DidNotReceive().Remove(Arg.Any<Product>());
    }
}
