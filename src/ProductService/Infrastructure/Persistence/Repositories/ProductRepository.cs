using ProductService.Domain.Abstractions;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ProductServiceDbContext context) : base(context)
    {
    }
}
