namespace LaTiendecicaEnLinea.Catalog.Dtos.Category.Responses;

/// <summary>
/// Represents a category response
/// </summary>
/// <param name="Id">Category identifier</param>
/// <param name="Name">Category name</param>
/// <param name="Description">Category description</param>
/// <param name="ProductCount">Number of products in this category</param>
public record CategoryResponse(
    int Id,
    string Name,
    string? Description,
    int ProductCount
);