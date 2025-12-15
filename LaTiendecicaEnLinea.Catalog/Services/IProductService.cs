using LaTiendecicaEnLinea.Catalog.Dtos.Product.Requests;
using LaTiendecicaEnLinea.Catalog.Dtos.Product.Responses;
using LaTiendecicaEnLinea.Shared.Common;

namespace LaTiendecicaEnLinea.Catalog.Services;

/// <summary>
/// Interface for product service operations
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Retrieves all products with optional filtering
    /// </summary>
    /// <param name="categoryId">Filter by category identifier</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="search">Search term for product name or description</param>
    /// <returns>Service result with list of products</returns>
    Task<ServiceResult<IEnumerable<ProductListResponse>>> GetAllAsync(
        int? categoryId = null,
        bool? isActive = null,
        string? search = null);

    /// <summary>
    /// Retrieves a product by identifier
    /// </summary>
    /// <param name="id">Product identifier</param>
    /// <returns>Service result with product details</returns>
    Task<ServiceResult<ProductResponse>> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="request">Product creation data</param>
    /// <returns>Service result with created product</returns>
    Task<ServiceResult<ProductResponse>> CreateAsync(CreateProductRequest request);

    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="id">Product identifier</param>
    /// <param name="request">Product update data</param>
    /// <returns>Service result with updated product</returns>
    Task<ServiceResult<ProductResponse>> UpdateAsync(int id, UpdateProductRequest request);

    /// <summary>
    /// Updates product stock quantity
    /// </summary>
    /// <param name="id">Product identifier</param>
    /// <param name="quantity">New stock quantity</param>
    /// <returns>Service result indicating operation status</returns>
    Task<ServiceResult> UpdateStockAsync(int id, int quantity);

    /// <summary>
    /// Deletes a product
    /// </summary>
    /// <param name="id">Product identifier</param>
    /// <returns>Service result indicating operation status</returns>
    Task<ServiceResult> DeleteAsync(int id);
}