using ProductService.Application.Features.Products.Commands.CreateProduct;

namespace ProductService.API.Extensions;

public static class ApiServiceExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<CreateProductCommandHandler>());

        return services;
    }
}
