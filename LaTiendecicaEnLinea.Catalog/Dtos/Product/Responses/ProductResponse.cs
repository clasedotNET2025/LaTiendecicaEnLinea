namespace LaTiendecicaEnLinea.Catalog.Dtos.Product.Responses;

/// <summary>
/// Represents a detailed product response
/// </summary>
/// <param name="Id">Product identifier</param>
/// <param name="Name">Product name</param>
/// <param name="Description">Product description</param>
/// <param name="Price">Product price</param>
/// <param name="Stock">Current stock quantity</param>
/// <param name="IsActive">Product active status</param>
/// <param name="CategoryId">Category identifier</param>
/// <param name="CategoryName">Name of the product's category</param>
public record ProductResponse(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    bool IsActive,
    int CategoryId,
    string CategoryName
);