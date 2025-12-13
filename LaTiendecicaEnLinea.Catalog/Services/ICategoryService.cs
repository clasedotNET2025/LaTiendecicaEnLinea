using LaTiendecicaEnLinea.Catalog.Dtos.Category;
using LaTiendecicaEnLinea.Shared.Common;

namespace LaTiendecicaEnLinea.Catalog.Services;

/// <summary>
/// Interface for category service operations
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Retrieves all categories
    /// </summary>
    /// <returns>Service result with list of categories</returns>
    Task<ServiceResult<IEnumerable<CategoryResponse>>> GetAllAsync();

    /// <summary>
    /// Retrieves a category by identifier
    /// </summary>
    /// <param name="id">Category identifier</param>
    /// <returns>Service result with category details</returns>
    Task<ServiceResult<CategoryResponse>> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new category
    /// </summary>
    /// <param name="request">Category creation data</param>
    /// <returns>Service result with created category</returns>
    Task<ServiceResult<CategoryResponse>> CreateAsync(CreateCategoryRequest request);

    /// <summary>
    /// Updates an existing category
    /// </summary>
    /// <param name="id">Category identifier</param>
    /// <param name="request">Category update data</param>
    /// <returns>Service result with updated category</returns>
    Task<ServiceResult<CategoryResponse>> UpdateAsync(int id, UpdateCategoryRequest request);

    /// <summary>
    /// Deletes a category
    /// </summary>
    /// <param name="id">Category identifier</param>
    /// <returns>Service result indicating operation status</returns>
    Task<ServiceResult> DeleteAsync(int id);
}