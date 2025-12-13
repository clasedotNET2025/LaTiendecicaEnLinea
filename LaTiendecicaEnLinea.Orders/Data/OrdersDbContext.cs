using Microsoft.EntityFrameworkCore;
using LaTiendecicaEnLinea.Orders.Entities;

namespace LaTiendecicaEnLinea.Orders.Data;

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(o => o.UserId);
            entity.HasIndex(o => o.OrderNumber).IsUnique();
            entity.HasIndex(o => o.Status);
            entity.HasIndex(o => o.OrderDate);

            entity.HasMany(o => o.OrderItems)
                  .WithOne(oi => oi.Order)
                  .HasForeignKey(oi => oi.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasIndex(oi => oi.ProductId);
            entity.Property(oi => oi.UnitPrice).HasPrecision(18, 2);
            entity.Property(oi => oi.Subtotal).HasPrecision(18, 2);
        });
    }
}