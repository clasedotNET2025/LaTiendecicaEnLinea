using LaTiendecicaEnLinea.Catalog.Data;
using LaTiendecicaEnLinea.Shared.Common;

namespace LaTiendecicaEnLinea.Catalog.Services;

/// <summary>
/// Service for stock management operations
/// </summary>
public class StockService : IStockService
{
    private readonly CatalogDbContext _context;

    /// <summary>
    /// Initializes a new instance of the StockService class
    /// </summary>
    /// <param name="context">Database context</param>
    public StockService(CatalogDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Reserves stock for a product
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <param name="quantity">Quantity to reserve</param>
    /// <returns>Service result indicating operation status</returns>
    public async Task<ServiceResult> ReserveStockAsync(int productId, int quantity)
    {
        try
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
            {
                return ServiceResult.NotFound($"Product with ID {productId} not found");
            }

            if (!product.IsActive)
            {
                return ServiceResult.ValidationError("Product is not active");
            }

            if (product.Stock < quantity)
            {
                return ServiceResult.ValidationError(
                    $"Insufficient stock. Available stock: {product.Stock}, requested quantity: {quantity}");
            }

            product.Stock -= quantity;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ServiceResult.Success($"Stock reserved. New stock: {product.Stock}");
        }
        catch (Exception ex)
        {
            return ServiceResult.Failure($"Error reserving stock: {ex.Message}");
        }
    }

    /// <summary>
    /// Releases previously reserved stock for a product
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <param name="quantity">Quantity to release</param>
    /// <returns>Service result indicating operation status</returns>
    public async Task<ServiceResult> ReleaseStockAsync(int productId, int quantity)
    {
        try
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
            {
                return ServiceResult.NotFound($"Product with ID {productId} not found");
            }

            product.Stock += quantity;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ServiceResult.Success($"Stock released. New stock: {product.Stock}");
        }
        catch (Exception ex)
        {
            return ServiceResult.Failure($"Error releasing stock: {ex.Message}");
        }
    }
}