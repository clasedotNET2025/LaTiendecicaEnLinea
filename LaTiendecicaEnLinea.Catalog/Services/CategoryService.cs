using LaTiendecicaEnLinea.Catalog.Data;
using LaTiendecicaEnLinea.Catalog.Dtos.Category.Requests;
using LaTiendecicaEnLinea.Catalog.Dtos.Category.Responses;
using LaTiendecicaEnLinea.Catalog.Entities;
using LaTiendecicaEnLinea.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace LaTiendecicaEnLinea.Catalog.Services;

/// <summary>
/// Service for category operations
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly CatalogDbContext _db;
    private readonly ILogger<CategoryService> _logger;

    /// <summary>
    /// Initializes a new instance of the CategoryService class
    /// </summary>
    /// <param name="db">Database context</param>
    /// <param name="logger">Logger instance</param>
    public CategoryService(CatalogDbContext db, ILogger<CategoryService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all categories
    /// </summary>
    /// <returns>Service result with list of categories</returns>
    public async Task<ServiceResult<IEnumerable<CategoryResponse>>> GetAllAsync()
    {
        var categories = await _db.Categories
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Description, c.Products.Count))
            .ToListAsync();

        return ServiceResult<IEnumerable<CategoryResponse>>.Success(categories);
    }

    /// <summary>
    /// Retrieves a category by identifier
    /// </summary>
    /// <param name="id">Category identifier</param>
    /// <returns>Service result with category details</returns>
    public async Task<ServiceResult<CategoryResponse>> GetByIdAsync(int id)
    {
        var category = await _db.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Description, c.Products.Count))
            .FirstOrDefaultAsync();

        if (category is null)
        {
            return ServiceResult<CategoryResponse>.Failure("Category not found");
        }

        return ServiceResult<CategoryResponse>.Success(category);
    }

    /// <summary>
    /// Creates a new category
    /// </summary>
    /// <param name="request">Category creation data</param>
    /// <returns>Service result with created category</returns>
    public async Task<ServiceResult<CategoryResponse>> CreateAsync(CreateCategoryRequest request)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Category created: {CategoryId} - {Name}", category.Id, category.Name);

        var response = new CategoryResponse(category.Id, category.Name, category.Description, 0);
        return ServiceResult<CategoryResponse>.Success(response, "Category created successfully");
    }

    /// <summary>
    /// Updates an existing category
    /// </summary>
    /// <param name="id">Category identifier</param>
    /// <param name="request">Category update data</param>
    /// <returns>Service result with updated category</returns>
    public async Task<ServiceResult<CategoryResponse>> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null)
        {
            return ServiceResult<CategoryResponse>.Failure("Category not found");
        }

        category.Name = request.Name;
        category.Description = request.Description;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Category updated: {CategoryId} - {Name}", category.Id, category.Name);

        var productCount = await _db.Products.CountAsync(p => p.CategoryId == id);
        var response = new CategoryResponse(category.Id, category.Name, category.Description, productCount);
        return ServiceResult<CategoryResponse>.Success(response, "Category updated successfully");
    }

    /// <summary>
    /// Deletes a category
    /// </summary>
    /// <param name="id">Category identifier</param>
    /// <returns>Service result indicating operation status</returns>
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var category = await _db.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
        {
            return ServiceResult.Failure("Category not found");
        }

        if (category.Products.Count > 0)
        {
            return ServiceResult.Failure("Cannot delete category with products");
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Category deleted: {CategoryId}", id);

        return ServiceResult.Success("Category deleted successfully");
    }
}