using LaTiendecicaEnLinea.Catalog.Data;
using LaTiendecicaEnLinea.Shared.Common;

namespace LaTiendecicaEnLinea.Catalog.Services
{
    public class StockService : IStockService
    {
        private readonly CatalogDbContext _context;

        public StockService(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult> ReserveStockAsync(int productId, int quantity)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);

                if (product == null)
                {
                    return ServiceResult.NotFound($"Producto con ID {productId} no encontrado");
                }

                if (!product.IsActive)
                {
                    return ServiceResult.ValidationError("El producto no está activo");
                }

                if (product.Stock < quantity)
                {
                    return ServiceResult.ValidationError(
                        $"Stock insuficiente. Stock disponible: {product.Stock}, cantidad solicitada: {quantity}");
                }

                product.Stock -= quantity;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ServiceResult.Success($"Stock reservado. Nuevo stock: {product.Stock}");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"Error al reservar stock: {ex.Message}");
            }
        }

        public async Task<ServiceResult> ReleaseStockAsync(int productId, int quantity)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);

                if (product == null)
                {
                    return ServiceResult.NotFound($"Producto con ID {productId} no encontrado");
                }

                product.Stock += quantity;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ServiceResult.Success($"Stock liberado. Nuevo stock: {product.Stock}");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"Error al liberar stock: {ex.Message}");
            }
        }
    }
}
