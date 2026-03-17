var builder = DistributedApplication.CreateBuilder(args);

var postgresPassword = builder.AddParameter("postgres-password", secret: true);

var postgres = builder
    .AddPostgres("postgres", password: postgresPassword)
    .WithPgAdmin()
    .WithDataVolume("postgres-data");
    //.WithHttpEndpoint(port: 5200, targetPort: 80);

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
