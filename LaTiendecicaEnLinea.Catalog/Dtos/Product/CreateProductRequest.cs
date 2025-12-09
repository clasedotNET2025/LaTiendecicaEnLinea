namespace LaTiendecicaEnLinea.Catalog.Dtos.Product
{
    public class CreateProductRequest
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required decimal Price { get; set; }
        public int Stock { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public required int CategoryId { get; set; }
    }
}
