using ProductService.Domain.Entities;

namespace ProductService.Domain.Abstractions;

public interface IProductRepository : IRepository<Product>
{
}
