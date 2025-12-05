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


var db = postgres.AddDatabase("identity-db");

var identity = builder.AddProject<Projects.LaTiendecicaEnLinea_Api_Identity>("latiendecicaenlinea-api-identity")
    .WaitFor(db)
    .WaitFor(rabbit)
    .WithReference(rabbit)
    .WithReference(db);


builder.AddProject<Projects.LaTiendecicaEnLinea_ApiGateway>("latiendecicaenlinea-apigateway")
    .WithReference(redis)
    .WithReference(identity)
    .WaitFor(redis)
    .WaitFor(identity);


builder.AddProject<Projects.LaTiendecicaEnLinea_Notifications>("latiendecicaenlinea-notifications")
    .WaitFor(rabbit)
    .WithReference(rabbit);


builder.Build().Run();