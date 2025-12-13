namespace LaTiendecicaEnLinea.Catalog.Dtos.Category;

/// <summary>
/// Request for creating a new category
/// </summary>
public class CreateCategoryRequest
{
    /// <summary>
    /// Category name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Category description
    /// </summary>
    public string? Description { get; set; }
}