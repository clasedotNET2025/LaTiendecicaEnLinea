namespace LaTiendecicaEnLinea.Catalog.Dtos.Category;

/// <summary>
/// Request for updating an existing category
/// </summary>
public class UpdateCategoryRequest
{
    /// <summary>
    /// Updated category name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Updated category description
    /// </summary>
    public string? Description { get; set; }
}