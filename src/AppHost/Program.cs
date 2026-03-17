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

var orderService = builder
    .AddProject<Projects.OrderService_API>("orderservice")
    .WithReference(orderDb)
    .WaitFor(orderDb)
    .WithReference(productService);

var gateway = builder
    .AddProject<Projects.Gateway>("gateway")
    .WithReference(productService)
    .WithReference(orderService)
    .WaitFor(productService)
    .WaitFor(orderService);

builder.AddExecutable("web", "npm", "../Web", "run", "dev")
    .WithReference(gateway)
    .WaitFor(gateway);

builder.Build().Run();
