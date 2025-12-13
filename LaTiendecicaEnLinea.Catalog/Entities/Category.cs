namespace LaTiendecicaEnLinea.Catalog.Entities;

/// <summary>
/// Represents a product category
/// </summary>
public class Category
{
    /// <summary>
    /// Category identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Category name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property for products in this category
    /// </summary>
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}