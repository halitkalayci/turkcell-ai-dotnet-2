var builder = DistributedApplication.CreateBuilder(args);

var productService = builder.AddProject<Projects.ProductService_API>("productservice");

builder.AddProject<Projects.OrderService_API>("orderservice")
       .WithReference(productService);

builder.Build().Run();
