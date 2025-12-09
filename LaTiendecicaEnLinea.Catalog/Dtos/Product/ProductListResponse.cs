namespace LaTiendecicaEnLinea.Catalog.Dtos.Product
{
    public record ProductListResponse(
        int Id,
        string Name,
        decimal Price,
        int Stock,
        bool IsActive,
        string CategoryName
    );
}
