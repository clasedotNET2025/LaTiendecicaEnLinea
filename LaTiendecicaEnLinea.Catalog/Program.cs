using Asp.Versioning;
using LaTiendecicaEnLinea.Catalog.Data;
using LaTiendecicaEnLinea.Catalog.Services;
using LaTiendecicaEnLinea.Identity.Extensions;
using LaTiendecicaEnLinea.Shared.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Configuration.AddUserSecrets(typeof(Program).Assembly, true);

builder.Services.AddControllers();

// Swagger con autenticación JWT
builder.Services.AddOpenApi("v1", options =>
{
    options.ConfigureDocumentInfo(
        "La Tiendecica - Catalog API V1",
        "v1",
        "Catalog API for managing products and categories");
    options.AddJwtBearerSecurity(); // Esto agrega el botón "Authorize" en Swagger
});

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

// Base de datos
builder.AddNpgsqlDbContext<CatalogDbContext>("catalogdb");

// Registrar servicios de negocio
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IStockService, StockService>();

// JWT Authentication (usa la extensión compartida)
builder.Services.AddJwtAuthentication(builder.Configuration);

// AUTHORIZATION: Configurar políticas de acceso basadas en roles
// ESTO ES NUEVO Y ES CLAVE PARA QUE [Authorize(Roles = "...")] FUNCIONE
builder.Services.AddAuthorization(options =>
{
    // Política que requiere rol Admin
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin")); // "Admin" debe coincidir con Roles.Admin

    // Política que requiere rol Customer
    options.AddPolicy("CustomerOnly", policy =>
        policy.RequireRole("Customer"));

    // Política para cualquier usuario autenticado
    options.AddPolicy("AuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Catalog API V1");
    });

    // Aplicar migraciones
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await context.Database.MigrateAsync();
}

app.UseHttpsRedirection();

// MIDDLEWARE EN ORDEN CORRECTO:
// 1. Authentication primero (valida el token JWT)
// 2. Authorization después (verifica roles/permissions)
app.UseAuthentication(); // Esto extrae el usuario del token JWT
app.UseAuthorization();  // Esto verifica si el usuario tiene acceso según [Authorize]

app.MapControllers();

await app.RunAsync();