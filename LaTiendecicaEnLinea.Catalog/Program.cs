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

builder.Services.AddOpenApi("v1", options =>
{
    options.ConfigureDocumentInfo(
        "La Tiendecica - Catalog API V1",
        "v1",
        "Catalog API for managing products and categories");
    options.AddJwtBearerSecurity();
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

builder.AddNpgsqlDbContext<CatalogDbContext>("catalogdb");

// Registrar servicios
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IStockService, StockService>();

// JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);


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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();