using LaTiendecicaEnLinea.Catalog.Data;
using LaTiendecicaEnLinea.Catalog.Dtos.Product;
using LaTiendecicaEnLinea.Catalog.Entities;
using LaTiendecicaEnLinea.Catalog.Services;
using LaTiendecicaEnLinea.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace LaTiendecicaEnLinea.Catalog.Services;

public class ProductService(CatalogDbContext db, ILogger<ProductService> logger) : IProductService
{
    public async Task<ServiceResult<IEnumerable<ProductListResponse>>> GetAllAsync(
        int? categoryId, bool? isActive, string? search)
    {
        var query = db.Products.Include(p => p.Category).AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || (p.Description != null && p.Description.Contains(search)));

        var products = await query
            .Select(p => new ProductListResponse(
                p.Id, p.Name, p.Price, p.Stock, p.IsActive, p.Category!.Name))
            .ToListAsync();

        return ServiceResult<IEnumerable<ProductListResponse>>.Success(products);
    }

    public async Task<ServiceResult<ProductResponse>> GetByIdAsync(int id)
    {
        var product = await db.Products
            .Include(p => p.Category)
            .Where(p => p.Id == id)
            .Select(p => new ProductResponse(
                p.Id, p.Name, p.Description, p.Price, p.Stock, p.IsActive, p.CategoryId, p.Category!.Name))
            .FirstOrDefaultAsync();

        if (product is null)
        {
            return ServiceResult<ProductResponse>.Failure("Product not found");
        }

        return ServiceResult<ProductResponse>.Success(product);
    }

    public async Task<ServiceResult<ProductResponse>> CreateAsync(CreateProductRequest request)
    {
        var categoryExists = await db.Categories.AnyAsync(c => c.Id == request.CategoryId);
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
            CategoryId = request.CategoryId
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        var category = await db.Categories.FindAsync(request.CategoryId);

        logger.LogInformation("Product created: {ProductId} - {Name}", product.Id, product.Name);

        var response = new ProductResponse(
            product.Id, product.Name, product.Description, product.Price,
            product.Stock, product.IsActive, product.CategoryId, category!.Name);

        return ServiceResult<ProductResponse>.Success(response, "Product created successfully");
    }

    public async Task<ServiceResult<ProductResponse>> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null)
        {
            return ServiceResult<ProductResponse>.Failure("Product not found");
        }

        var categoryExists = await db.Categories.AnyAsync(c => c.Id == request.CategoryId);
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

        await db.SaveChangesAsync();

        var category = await db.Categories.FindAsync(request.CategoryId);

        logger.LogInformation("Product updated: {ProductId} - {Name}", product.Id, product.Name);

        var response = new ProductResponse(
            product.Id, product.Name, product.Description, product.Price,
            product.Stock, product.IsActive, product.CategoryId, category!.Name);

        return ServiceResult<ProductResponse>.Success(response, "Product updated successfully");
    }

    public async Task<ServiceResult> UpdateStockAsync(int id, int quantity)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null)
        {
            return ServiceResult.Failure("Product not found");
        }

        product.Stock = quantity;
        product.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        logger.LogInformation("Product stock updated: {ProductId} - New stock: {Stock}", id, quantity);

        return ServiceResult.Success("Stock updated successfully");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null)
        {
            return ServiceResult.Failure("Product not found");
        }

        db.Products.Remove(product);
        await db.SaveChangesAsync();

        logger.LogInformation("Product deleted: {ProductId}", id);

        return ServiceResult.Success("Product deleted successfully");
    }
}