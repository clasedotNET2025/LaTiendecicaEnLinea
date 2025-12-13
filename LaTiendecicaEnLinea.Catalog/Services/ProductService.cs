using LaTiendecicaEnLinea.Catalog.Data;
using LaTiendecicaEnLinea.Catalog.Dtos.Product;
using LaTiendecicaEnLinea.Catalog.Entities;
using LaTiendecicaEnLinea.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace LaTiendecicaEnLinea.Catalog.Services;

/// <summary>
/// Service for product operations
/// </summary>
public class ProductService : IProductService
{
    private readonly CatalogDbContext _db;
    private readonly ILogger<ProductService> _logger;

    /// <summary>
    /// Initializes a new instance of the ProductService class
    /// </summary>
    /// <param name="db">Database context</param>
    /// <param name="logger">Logger instance</param>
    public ProductService(CatalogDbContext db, ILogger<ProductService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all products with optional filtering
    /// </summary>
    /// <param name="categoryId">Filter by category identifier</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="search">Search term for product name or description</param>
    /// <returns>Service result with list of products</returns>
    public async Task<ServiceResult<IEnumerable<ProductListResponse>>> GetAllAsync(
        int? categoryId, bool? isActive, string? search)
    {
        var query = _db.Products.Include(p => p.Category).AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) ||
                (p.Description != null && p.Description.Contains(search)));

        var products = await query
            .Select(p => new ProductListResponse(
                p.Id, p.Name, p.Price, p.Stock, p.IsActive, p.Category!.Name))
            .ToListAsync();

        return ServiceResult<IEnumerable<ProductListResponse>>.Success(products);
    }

    /// <summary>
    /// Retrieves a product by identifier
    /// </summary>
    /// <param name="id">Product identifier</param>
    /// <returns>Service result with product details</returns>
    public async Task<ServiceResult<ProductResponse>> GetByIdAsync(int id)
    {
        var product = await _db.Products
            .Include(p => p.Category)
            .Where(p => p.Id == id)
            .Select(p => new ProductResponse(
                p.Id, p.Name, p.Description, p.Price, p.Stock,
                p.IsActive, p.CategoryId, p.Category!.Name))
            .FirstOrDefaultAsync();

        if (product is null)
        {
            return ServiceResult<ProductResponse>.Failure("Product not found");
        }

        return ServiceResult<ProductResponse>.Success(product);
    }

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="request">Product creation data</param>
    /// <returns>Service result with created product</returns>
    public async Task<ServiceResult<ProductResponse>> CreateAsync(CreateProductRequest request)
    {
        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
        {
            return ServiceResult<ProductResponse>.Failure("Category not found");
        }

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            IsActive = request.IsActive,
            CategoryId = request.CategoryId
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var category = await _db.Categories.FindAsync(request.CategoryId);

        _logger.LogInformation("Product created: {ProductId} - {Name}", product.Id, product.Name);

        var response = new ProductResponse(
            product.Id, product.Name, product.Description, product.Price,
            product.Stock, product.IsActive, product.CategoryId, category!.Name);

        return ServiceResult<ProductResponse>.Success(response, "Product created successfully");
    }

    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="id">Product identifier</param>
    /// <param name="request">Product update data</param>
    /// <returns>Service result with updated product</returns>
    public async Task<ServiceResult<ProductResponse>> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null)
        {
            return ServiceResult<ProductResponse>.Failure("Product not found");
        }

        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
        {
            return ServiceResult<ProductResponse>.Failure("Category not found");
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.IsActive = request.IsActive;
        product.CategoryId = request.CategoryId;
        product.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var category = await _db.Categories.FindAsync(request.CategoryId);

        _logger.LogInformation("Product updated: {ProductId} - {Name}", product.Id, product.Name);

        var response = new ProductResponse(
            product.Id, product.Name, product.Description, product.Price,
            product.Stock, product.IsActive, product.CategoryId, category!.Name);

        return ServiceResult<ProductResponse>.Success(response, "Product updated successfully");
    }

    /// <summary>
    /// Updates product stock quantity
    /// </summary>
    /// <param name="id">Product identifier</param>
    /// <param name="quantity">New stock quantity</param>
    /// <returns>Service result indicating operation status</returns>
    public async Task<ServiceResult> UpdateStockAsync(int id, int quantity)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null)
        {
            return ServiceResult.Failure("Product not found");
        }

        product.Stock = quantity;
        product.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Product stock updated: {ProductId} - New stock: {Stock}", id, quantity);

        return ServiceResult.Success("Stock updated successfully");
    }

    /// <summary>
    /// Deletes a product
    /// </summary>
    /// <param name="id">Product identifier</param>
    /// <returns>Service result indicating operation status</returns>
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null)
        {
            return ServiceResult.Failure("Product not found");
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Product deleted: {ProductId}", id);

        return ServiceResult.Success("Product deleted successfully");
    }
}