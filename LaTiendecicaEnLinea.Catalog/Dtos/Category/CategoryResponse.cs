namespace LaTiendecicaEnLinea.Catalog.Dtos.Category
{
    public record CategoryResponse(
        int Id,
        string Name,
        string? Description,
        int ProductCount
    );
}