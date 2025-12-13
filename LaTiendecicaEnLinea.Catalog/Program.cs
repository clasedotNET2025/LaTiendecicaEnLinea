using Asp.Versioning;
using LaTiendecicaEnLinea.Catalog.Data;
using LaTiendecicaEnLinea.Catalog.Services;
using LaTiendecicaEnLinea.Identity.Extensions;
using LaTiendecicaEnLinea.Shared.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Configuration.AddUserSecrets(typeof(Program).Assembly, true);

// Debug: Verify JWT configuration is loaded
var jwtSecret = builder.Configuration["Jwt:Secret"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

Console.WriteLine("=== CATALOG JWT CONFIGURATION ===");
Console.WriteLine($"Secret configured: {!string.IsNullOrEmpty(jwtSecret)}");
Console.WriteLine($"Issuer: {jwtIssuer}");
Console.WriteLine($"Audience: {jwtAudience}");
Console.WriteLine("==================================");

builder.Services.AddControllers();

// Swagger with JWT authentication support
builder.Services.AddOpenApi("v1", options =>
{
    options.ConfigureDocumentInfo(
        "La Tiendecica - Catalog API V1",
        "v1",
        "Catalog API for managing products and categories");
    options.AddJwtBearerSecurity();
});

// API versioning configuration
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

// Database configuration
builder.AddNpgsqlDbContext<CatalogDbContext>("catalogdb");

// Business services registration
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IStockService, StockService>();

// JWT Authentication using shared extension
builder.Services.AddJwtAuthentication(builder.Configuration);

// Authorization policies for role-based access control
builder.Services.AddAuthorization(options =>
{
    // Requires Admin role
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    // Requires Customer role  
    options.AddPolicy("CustomerOnly", policy =>
        policy.RequireRole("Customer"));

    // Requires any authenticated user
    options.AddPolicy("AuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Catalog API V1");
    });

    // Apply database migrations
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await context.Database.MigrateAsync();
}

app.UseHttpsRedirection();

// Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();