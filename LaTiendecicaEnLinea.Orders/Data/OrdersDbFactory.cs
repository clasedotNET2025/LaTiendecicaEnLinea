using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LaTiendecicaEnLinea.Orders.Data;

/// <summary>
/// Factory for creating OrdersDbContext instances at design time
/// </summary>
public class OrdersDbContextFactory : IDesignTimeDbContextFactory<OrdersDbContext>
{
    /// <summary>
    /// Creates a new instance of OrdersDbContext for design-time operations
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>Configured OrdersDbContext instance</returns>
    public OrdersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrdersDbContext>();

        // Connection string for design-time operations (migrations, etc.)
        // IMPORTANT: This should match the connection string in appsettings/apphost
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=ordersdb;Username=postgres;Password=postgres",
            options => options.MigrationsAssembly(typeof(OrdersDbContext).Assembly.FullName));

        return new OrdersDbContext(optionsBuilder.Options);
    }
}