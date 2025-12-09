namespace LaTiendecicaEnLinea.Catalog.Dtos.Product
{
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
}
