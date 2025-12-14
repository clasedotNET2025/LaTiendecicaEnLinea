
var builder = DistributedApplication.CreateBuilder(args);

var mailServer = builder
    .AddContainer("maildev", "maildev/maildev:latest")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithHttpEndpoint(port: 1080, targetPort: 1080, name: "web")
    .WithEndpoint(port: 1025, targetPort: 1025, name: "smtp");

var rabbit = builder
    .AddRabbitMQ("rabbitmq")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("rabbitmq-data")
    .WithManagementPlugin();

var postgres = builder
    .AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("postgres")
    .WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(5050));

var redis = builder.AddRedis("redis")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("redis-data-identity")
    .WithRedisInsight();

// Identity Database
var identityDb = postgres.AddDatabase("identity");

var identity = builder.AddProject<Projects.LaTiendecicaEnLinea_Api_Identity>("latiendecicaenlinea-api-identity")
    .WaitFor(identityDb)
    .WaitFor(rabbit)
    .WithReference(rabbit)
    .WithReference(identityDb);

// Catalog Database
var catalogDb = postgres.AddDatabase("catalogdb");

var catalog = builder.AddProject<Projects.LaTiendecicaEnLinea_Catalog>("latiendecicaenlinea-catalog")
    .WaitFor(catalogDb)
    .WithReference(catalogDb)
    .WaitFor(rabbit)
    .WithReference(rabbit);

// Orders Database
var ordersDb = postgres.AddDatabase("ordersdb");

var orders = builder.AddProject<Projects.LaTiendecicaEnLinea_Orders>("latiendecicaenlinea-orders")
    .WaitFor(ordersDb)
    .WaitFor(catalog)
    .WaitFor(rabbit)
    .WithReference(ordersDb)
    .WithReference(catalog)
    .WithReference(rabbit);

var gateway = builder.AddProject<Projects.LaTiendecicaEnLinea_ApiGateway>("latiendecicaenlinea-apigateway")
    .WithReference(redis)
    .WithReference(identity)
    .WithReference(catalog)
    .WithReference(orders)
    .WaitFor(redis)
    .WaitFor(identity)
    .WaitFor(catalog)
    .WaitFor(orders);

builder.AddProject<Projects.LaTiendecicaEnLinea_Notifications>("latiendecicaenlinea-notifications")
    .WaitFor(rabbit)
    .WithReference(rabbit);

builder.Build().Run();