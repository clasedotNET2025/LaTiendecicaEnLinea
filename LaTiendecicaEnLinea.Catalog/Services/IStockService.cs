using LaTiendecicaEnLinea.Shared.Common;

namespace LaTiendecicaEnLinea.Catalog.Services;

/// <summary>
/// Interface for stock management operations
/// </summary>
public interface IStockService
{
    /// <summary>
    /// Reserves stock for a product
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <param name="quantity">Quantity to reserve</param>
    /// <returns>Service result indicating operation status</returns>
    Task<ServiceResult> ReserveStockAsync(int productId, int quantity);

    /// <summary>
    /// Releases previously reserved stock for a product
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <param name="quantity">Quantity to release</param>
    /// <returns>Service result indicating operation status</returns>
    Task<ServiceResult> ReleaseStockAsync(int productId, int quantity);
}