using Microsoft.EntityFrameworkCore;
using LaTiendecicaEnLinea.Orders.Entities;

namespace LaTiendecicaEnLinea.Orders.Data;

/// <summary>
/// Database context for Orders microservice
/// </summary>
public class OrdersDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the OrdersDbContext class
    /// </summary>
    /// <param name="options">Database context options</param>
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the Orders database set
    /// </summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <summary>
    /// Gets or sets the OrderItems database set
    /// </summary>
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    /// <summary>
    /// Configures the database model
    /// </summary>
    /// <param name="modelBuilder">Model builder instance</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // PostgreSQL specific configuration
        modelBuilder.HasPostgresExtension("uuid-ossp");

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Configuración para GUID (NO UseIdentityAlwaysColumn)
            entity.Property(e => e.Id)
                .HasColumnType("uuid")  // Tipo PostgreSQL para GUID
                .HasDefaultValueSql("gen_random_uuid()")  // Genera GUID automático
                .ValueGeneratedOnAdd();

            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TotalAmount)
                .HasPrecision(18, 2);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(e => e.CustomerNotes).HasMaxLength(1000);
            entity.Property(e => e.AdminNotes).HasMaxLength(1000);

            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.OrderDate);

            // Relationship with OrderItems
            entity.HasMany(o => o.OrderItems)
                  .WithOne(oi => oi.Order)
                  .HasForeignKey(oi => oi.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // OrderItem configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Configuración para GUID (NO UseIdentityAlwaysColumn)
            entity.Property(e => e.Id)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.UnitPrice)
                .HasPrecision(18, 2);
            entity.Property(e => e.Subtotal)
                .HasPrecision(18, 2);

            entity.HasIndex(e => e.ProductId);
        });

        base.OnModelCreating(modelBuilder);
    }
}