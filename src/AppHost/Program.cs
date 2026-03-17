var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ProductService_API>("productservice");
builder.AddProject<Projects.OrderService_API>("orderservice");

builder.Build().Run();
