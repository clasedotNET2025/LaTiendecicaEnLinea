using LaTiendecicaEnLinea.Catalog.Dtos.Product;
using LaTiendecicaEnLinea.Catalog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaTiendecicaEnLinea.Catalog.Controllers;

/// <summary>
/// Controller for managing products
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize] // Require authentication by default for all endpoints
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    /// <summary>
    /// Initializes a new instance of the ProductsController class
    /// </summary>
    /// <param name="productService">Service for product operations</param>
    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Retrieves all products with optional filtering
    /// </summary>
    /// <param name="categoryId">Filter by category identifier</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="search">Search term for product name or description</param>
    /// <returns>List of filtered products</returns>
    [HttpGet]
    [AllowAnonymous] // Public endpoint - no authentication required
    [ProducesResponseType(typeof(IEnumerable<ProductListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductListResponse>>> GetAll(
        [FromQuery] int? categoryId,
        [FromQuery] bool? isActive,
        [FromQuery] string? search)
    {
        var result = await _productService.GetAllAsync(categoryId, isActive, search);

        if (!result.Succeded)
            return StatusCode(result.StatusCode, result.Message);

        return Ok(result.Data);
    }

    /// <summary>
    /// Retrieves a specific product by ID
    /// </summary>
    /// <param name="id">Product identifier</param>
    /// <returns>The requested product</returns>
    [HttpGet("{id:int}")]
    [AllowAnonymous] // Public endpoint - no authentication required
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductResponse>> GetById(int id)
    {
        var result = await _productService.GetByIdAsync(id);

        if (!result.Succeded)
            return StatusCode(result.StatusCode, result.Message);

        return Ok(result.Data);
    }

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="request">Product creation data</param>
    /// <returns>The newly created product</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")] // Only users with Admin role can create products
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request)
    {
        var result = await _productService.CreateAsync(request);

        if (!result.Succeded)
            return StatusCode(result.StatusCode, result.Message);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="id">Product identifier</param>
    /// <param name="request">Product update data</param>
    /// <returns>The updated product</returns>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")] // Only users with Admin role can update products
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductResponse>> Update(int id, UpdateProductRequest request)
    {
        var result = await _productService.UpdateAsync(id, request);

        if (!result.Succeded)
            return StatusCode(result.StatusCode, result.Message);

        return Ok(result.Data);
    }

    /// <summary>
    /// Updates product stock quantity
    /// </summary>
    /// <param name="id">Product identifier</param>
    /// <param name="quantity">New stock quantity</param>
    [HttpPatch("{id:int}/stock")]
    [Authorize(Roles = "Admin")] // Only users with Admin role can update stock
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] int quantity)
    {
        var result = await _productService.UpdateStockAsync(id, quantity);

        if (!result.Succeded)
            return StatusCode(result.StatusCode, result.Message);

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Deletes a product
    /// </summary>
    /// <param name="id">Product identifier</param>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")] // Only users with Admin role can delete products
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _productService.DeleteAsync(id);

        if (!result.Succeded)
            return StatusCode(result.StatusCode, result.Message);

        return NoContent();
    }
}