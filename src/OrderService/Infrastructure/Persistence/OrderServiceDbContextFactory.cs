using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderService.Infrastructure.Persistence;

public sealed class OrderServiceDbContextFactory : IDesignTimeDbContextFactory<OrderServiceDbContext>
{
    public OrderServiceDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__orderdb")
            ?? "Host=localhost;Port=5432;Database=orderdb;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<OrderServiceDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new OrderServiceDbContext(options);
    }
}
