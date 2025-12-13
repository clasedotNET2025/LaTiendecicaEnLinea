namespace LaTiendecicaEnLinea.Catalog.Dtos.Product;

/// <summary>
/// Represents a product in list view
/// </summary>
/// <param name="Id">Product identifier</param>
/// <param name="Name">Product name</param>
/// <param name="Price">Product price</param>
/// <param name="Stock">Current stock quantity</param>
/// <param name="IsActive">Product active status</param>
/// <param name="CategoryName">Name of the product's category</param>
public record ProductListResponse(
    int Id,
    string Name,
    decimal Price,
    int Stock,
    bool IsActive,
    string CategoryName
);