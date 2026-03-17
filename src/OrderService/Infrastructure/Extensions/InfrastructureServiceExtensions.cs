using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Abstractions;
using OrderService.Domain.Abstractions;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Persistence.Repositories;

namespace OrderService.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddDbContext<OrderServiceDbContext>(options =>
            options.UseInMemoryDatabase("OrderServiceDb"));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHttpClient<IProductServiceClient, ProductServiceClient>(client =>
            client.BaseAddress = new Uri("https+http://productservice"));

        return services;
    }
}
