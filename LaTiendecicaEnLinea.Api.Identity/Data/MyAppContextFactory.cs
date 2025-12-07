using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LaTiendecicaEnLinea.Api.Identity.Data
{
    public class MyAppContextFactory : IDesignTimeDbContextFactory<MyAppContext>
    {
        public MyAppContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MyAppContext>();

            // Use a placeholder connection string for migrations
            // The actual connection string will come from Aspire at runtime
            optionsBuilder.UseNpgsql("Host=localhost;Database=identity;Username=postgres;Password=postgres");

            return new MyAppContext(optionsBuilder.Options);
        }
    }
}
