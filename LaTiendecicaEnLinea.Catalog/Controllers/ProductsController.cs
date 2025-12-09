// Controllers/ProductsController.cs
using LaTiendecicaEnLinea.Catalog.Dtos.Product;
using LaTiendecicaEnLinea.Catalog.Services;
using Microsoft.AspNetCore.Mvc;

namespace LaTiendecicaEnLinea.Catalog.Controllers;

[ApiController]
[Route("api/v1/[controller]")]  // Añadí v1 para consistencia
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
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

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetById(int id)
    {
        var result = await _productService.GetByIdAsync(id);

        if (!result.Succeded)
            return StatusCode(result.StatusCode, result.Message);

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request)
    {
        var result = await _productService.CreateAsync(request);

        if (!result.Succeded)
            return StatusCode(result.StatusCode, result.Message);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponse>> Update(int id, UpdateProductRequest request)
    {
        var result = await _productService.UpdateAsync(id, request);

        if (!result.Succeded)
            return StatusCode(result.StatusCode, result.Message);

        return Ok(result.Data);
    }

    [HttpPatch("{id:int}/stock")]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] int quantity)  // Cambié a parámetro simple
    {
        var result = await _productService.UpdateStockAsync(id, quantity);

        if (!result.Succeded)
            return StatusCode(result.StatusCode, result.Message);

        return Ok(new { message = result.Message });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _productService.DeleteAsync(id);

        if (!result.Succeded)
            return StatusCode(result.StatusCode, result.Message);

        return NoContent();
    }
}