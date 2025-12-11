namespace LaTiendecicaEnLinea.Catalog.Dtos.Category
{
    public class CreateCategoryRequest
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}