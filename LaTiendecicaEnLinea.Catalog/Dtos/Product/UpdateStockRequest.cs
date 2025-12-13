namespace LaTiendecicaEnLinea.Catalog.Dtos.Product;

/// <summary>
/// Request for updating product stock
/// </summary>
public class UpdateStockRequest
{
    /// <summary>
    /// New stock quantity
    /// </summary>
    public required int Quantity { get; set; }
}