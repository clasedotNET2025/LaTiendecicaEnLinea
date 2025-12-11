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

// Catalog Database - FIXED
var catalogDb = postgres.AddDatabase("catalogdb");

var catalog = builder.AddProject<Projects.LaTiendecicaEnLinea_Catalog>("latiendecicaenlinea-catalog")
    .WaitFor(catalogDb)
    .WithReference(catalogDb)  // This adds the connection string automatically
    .WaitFor(rabbit)  // Add if Catalog uses RabbitMQ
    .WithReference(rabbit);  // Add if Catalog uses RabbitMQ

builder.AddProject<Projects.LaTiendecicaEnLinea_ApiGateway>("latiendecicaenlinea-apigateway")
    .WithReference(redis)
    .WithReference(identity)
    .WithReference(catalog)  // Add reference to catalog if gateway needs it
    .WaitFor(redis)
    .WaitFor(identity)
    .WaitFor(catalog);

builder.AddProject<Projects.LaTiendecicaEnLinea_Notifications>("latiendecicaenlinea-notifications")
    .WaitFor(rabbit)
    .WithReference(rabbit);

builder.Build().Run();