using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderService.Application.Abstractions;
using OrderService.Domain.Abstractions;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Persistence.Repositories;

namespace OrderService.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IHostApplicationBuilder AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<OrderServiceDbContext>("orderdb");

        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        builder.Services.AddHttpClient<IProductServiceClient, ProductServiceClient>(client =>
            client.BaseAddress = new Uri("https+http://productservice"));

        return builder;
    }
}
