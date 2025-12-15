namespace LaTiendecicaEnLinea.Catalog.Dtos.Product.Requests;

/// <summary>
/// Request for stock operations
/// </summary>
public class StockOperationRequest
{
    /// <summary>
    /// Quantity to add or remove from stock
    /// </summary>
    public required int Quantity { get; set; }
}