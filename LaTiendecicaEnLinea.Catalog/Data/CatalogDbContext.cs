// Data/CatalogDbContext.cs - VERSIÓN MEJORADA (sin Identity)
using LaTiendecicaEnLinea.Catalog.Entities;
using Microsoft.EntityFrameworkCore;

namespace LaTiendecicaEnLinea.Catalog.Data;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuración específica para PostgreSQL
        modelBuilder.HasPostgresExtension("uuid-ossp");

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.Name).IsUnique();

            // Para PostgreSQL - autoincrement (IGUAL que Identity)
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

            // Para PostgreSQL - autoincrement (IGUAL que Identity)
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .ValueGeneratedOnAdd();

            // Relationship
            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(p => p.IsActive);
            entity.HasIndex(p => p.CategoryId);
        });

        // Asegúrate de llamar al base
        base.OnModelCreating(modelBuilder);
    }
}