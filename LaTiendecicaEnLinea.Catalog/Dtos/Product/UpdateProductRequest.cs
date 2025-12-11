namespace LaTiendecicaEnLinea.Catalog.Dtos.Product
{
    public class UpdateProductRequest
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required decimal Price { get; set; }
        public required int Stock { get; set; }
        public required bool IsActive { get; set; }
        public required int CategoryId { get; set; }
    }
}
