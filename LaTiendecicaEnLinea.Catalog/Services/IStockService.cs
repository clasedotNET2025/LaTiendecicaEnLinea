using LaTiendecicaEnLinea.Shared.Common;

namespace LaTiendecicaEnLinea.Catalog.Services
{
    public interface IStockService
    {
        Task<ServiceResult> ReserveStockAsync(int productId, int quantity);
        Task<ServiceResult> ReleaseStockAsync(int productId, int quantity);
    }
}
