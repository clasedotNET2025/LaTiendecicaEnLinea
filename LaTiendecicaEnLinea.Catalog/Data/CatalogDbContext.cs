using LaTiendecicaEnLinea.Catalog.Entities;
using Microsoft.EntityFrameworkCore;

namespace LaTiendecicaEnLinea.Catalog.Data;

/// <summary>
/// Database context for Catalog microservice
/// </summary>
public class CatalogDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the CatalogDbContext class
    /// </summary>
    /// <param name="options">Database context options</param>
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the Categories database set
    /// </summary>
    public DbSet<Category> Categories => Set<Category>();

    /// <summary>
    /// Gets or sets the Products database set
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>
    /// Configures the database model
    /// </summary>
    /// <param name="modelBuilder">Model builder instance</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // PostgreSQL specific configuration
        modelBuilder.HasPostgresExtension("uuid-ossp");

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.Name).IsUnique();

            // PostgreSQL auto-increment configuration
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .ValueGeneratedOnAdd();
        });

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Price)
                .HasPrecision(18, 2);

            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.CategoryId);

            // PostgreSQL auto-increment configuration
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .ValueGeneratedOnAdd();

            // Relationship with Category
            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }
}