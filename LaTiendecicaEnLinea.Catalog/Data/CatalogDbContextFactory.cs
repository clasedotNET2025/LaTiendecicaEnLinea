using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LaTiendecicaEnLinea.Catalog.Data;

/// <summary>
/// Factory for creating CatalogDbContext instances at design time
/// </summary>
public class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    /// <summary>
    /// Creates a new instance of CatalogDbContext for design-time operations
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>Configured CatalogDbContext instance</returns>
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();

        // Connection string for design-time operations (migrations, etc.)
        // IMPORTANT: This should match the connection string in appsettings/apphost
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=catalogdb;Username=postgres;Password=postgres",
            options => options.MigrationsAssembly(typeof(CatalogDbContext).Assembly.FullName));

        return new CatalogDbContext(optionsBuilder.Options);
    }
}