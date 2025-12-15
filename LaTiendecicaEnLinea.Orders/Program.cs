using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using LaTiendecicaEnLinea.Orders.Data;
using LaTiendecicaEnLinea.Orders.Services;
using LaTiendecicaEnLinea.Identity.Extensions;
using LaTiendecicaEnLinea.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets(typeof(Program).Assembly, true);

// Debug: Verify JWT configuration is loaded
Console.WriteLine("=== ORDERS JWT CONFIGURATION ===");
Console.WriteLine($"Secret: {!string.IsNullOrEmpty(builder.Configuration["Jwt:Secret"])}");
Console.WriteLine($"Issuer: {builder.Configuration["Jwt:Issuer"]}");
Console.WriteLine($"Audience: {builder.Configuration["Jwt:Audience"]}");
Console.WriteLine("==================================");

builder.Services.AddControllers();

// Swagger with JWT authentication support
builder.Services.AddOpenApi("v1", options =>
{
    options.ConfigureDocumentInfo(
        "La Tiendecica - Orders API V1",
        "v1",
        "Orders API for managing customer orders");
    options.AddJwtBearerSecurity();
});

// API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

// Database
builder.AddNpgsqlDbContext<OrdersDbContext>("ordersdb");

// Services
builder.Services.AddScoped<IOrderService, OrderService>();

// JWT Authentication - ¡IMPORTANTE!
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // PRIMERO: Authentication y Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // LUEGO: Swagger
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Orders API V1");

        // Configura Swagger UI para usar JWT
        options.OAuthClientId("swagger-ui");
        options.OAuthAppName("Orders API - Swagger UI");
        options.OAuthUsePkce();
    });

    // Apply database migrations
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    await context.Database.MigrateAsync();
}
else
{
    // En producción también necesitas esto
    app.UseAuthentication();
    app.UseAuthorization();
}

app.UseHttpsRedirection();

app.MapControllers();

await app.RunAsync();