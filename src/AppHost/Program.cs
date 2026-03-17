var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("postgres") // addContainer();
    .WithPgAdmin();

var productDb = postgres.AddDatabase("productdb");
var orderDb = postgres.AddDatabase("orderdb");

var productService = builder
    .AddProject<Projects.ProductService_API>("productservice")
    .WithReference(productDb)
    .WaitFor(productDb);

builder
    .AddProject<Projects.OrderService_API>("orderservice")
    .WithReference(orderDb)
    .WaitFor(orderDb)
    .WithReference(productService);

builder.Build().Run();
