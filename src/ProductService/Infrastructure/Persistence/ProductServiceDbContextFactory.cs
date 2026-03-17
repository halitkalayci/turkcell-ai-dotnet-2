using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProductService.Infrastructure.Persistence;

public sealed class ProductServiceDbContextFactory : IDesignTimeDbContextFactory<ProductServiceDbContext>
{
    public ProductServiceDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__productdb")
            ?? "Host=localhost;Port=5432;Database=productdb;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<ProductServiceDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ProductServiceDbContext(options);
    }
}
