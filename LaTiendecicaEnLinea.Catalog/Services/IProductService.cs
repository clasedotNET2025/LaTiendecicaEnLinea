using LaTiendecicaEnLinea.Catalog.Dtos.Product;
using LaTiendecicaEnLinea.Shared.Common;

namespace LaTiendecicaEnLinea.Catalog.Services
{
    public interface IProductService
    {
        Task<ServiceResult<IEnumerable<ProductListResponse>>> GetAllAsync(
            int? categoryId = null,
            bool? isActive = null,
            string? search = null);

        Task<ServiceResult<ProductResponse>> GetByIdAsync(int id);
        Task<ServiceResult<ProductResponse>> CreateAsync(CreateProductRequest request);
        Task<ServiceResult<ProductResponse>> UpdateAsync(int id, UpdateProductRequest request);
        Task<ServiceResult> UpdateStockAsync(int id, int quantity);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
