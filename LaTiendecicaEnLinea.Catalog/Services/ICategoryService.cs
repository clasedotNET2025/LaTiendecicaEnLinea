using LaTiendecicaEnLinea.Catalog.Dtos.Category;
using LaTiendecicaEnLinea.Shared.Common;

namespace LaTiendecicaEnLinea.Catalog.Services
{
    public interface ICategoryService
    {
        Task<ServiceResult<IEnumerable<CategoryResponse>>> GetAllAsync();
        Task<ServiceResult<CategoryResponse>> GetByIdAsync(int id);
        Task<ServiceResult<CategoryResponse>> CreateAsync(CreateCategoryRequest request);
        Task<ServiceResult<CategoryResponse>> UpdateAsync(int id, UpdateCategoryRequest request);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
