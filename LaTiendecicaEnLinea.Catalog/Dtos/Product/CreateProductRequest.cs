namespace LaTiendecicaEnLinea.Catalog.Dtos.Product;

/// <summary>
/// Request for creating a new product
/// </summary>
public class CreateProductRequest
{
    /// <summary>
    /// Product name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Product description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Product price
    /// </summary>
    public required decimal Price { get; set; }

    /// <summary>
    /// Initial stock quantity
    /// </summary>
    public int Stock { get; set; } = 0;

    /// <summary>
    /// Indicates if the product is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Category identifier
    /// </summary>
    public required int CategoryId { get; set; }
}