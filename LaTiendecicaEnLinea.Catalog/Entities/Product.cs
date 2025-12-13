namespace LaTiendecicaEnLinea.Catalog.Entities;

/// <summary>
/// Represents a product
/// </summary>
public class Product
{
    /// <summary>
    /// Product identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Product name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Product price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Current stock quantity
    /// </summary>
    public int Stock { get; set; } = 0;

    /// <summary>
    /// Indicates if the product is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Category identifier
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property for the product's category
    /// </summary>
    public virtual Category? Category { get; set; }
}