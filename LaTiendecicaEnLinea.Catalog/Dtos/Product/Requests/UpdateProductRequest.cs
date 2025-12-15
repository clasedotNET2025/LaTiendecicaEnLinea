namespace LaTiendecicaEnLinea.Catalog.Dtos.Product.Requests;

/// <summary>
/// Request for updating an existing product
/// </summary>
public class UpdateProductRequest
{
    /// <summary>
    /// Updated product name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Updated product description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Updated product price
    /// </summary>
    public required decimal Price { get; set; }

    /// <summary>
    /// Updated stock quantity
    /// </summary>
    public required int Stock { get; set; }

    /// <summary>
    /// Updated product active status
    /// </summary>
    public required bool IsActive { get; set; }

    /// <summary>
    /// Updated category identifier
    /// </summary>
    public required int CategoryId { get; set; }
}