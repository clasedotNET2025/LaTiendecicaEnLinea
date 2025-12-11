// CatalogDbContextFactory.cs - HAZLO IGUAL QUE IDENTITY
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LaTiendecicaEnLinea.Catalog.Data;

public class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();

        optionsBuilder.UseNpgsql("Host=localhost;Database=catalogdb;Username=postgres;Password=postgres");

        return new CatalogDbContext(optionsBuilder.Options);
    }
}