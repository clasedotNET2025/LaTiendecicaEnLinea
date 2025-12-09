// Controllers/CategoriesController.cs
using LaTiendecicaEnLinea.Catalog.Dtos.Category;
using LaTiendecicaEnLinea.Catalog.Services;
using Microsoft.AspNetCore.Mvc;

namespace LaTiendecicaEnLinea.Catalog.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetAll()
    {
        var result = await _categoryService.GetAllAsync();
        return Ok(result.Data);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryResponse>> GetById(int id)
    {
        var result = await _categoryService.GetByIdAsync(id);

        if (!result.Succeded)  // Note: Succeded (dos 'c')
            return NotFound(result.Message);  // Usa Message, no Errors

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create(CreateCategoryRequest request)
    {
        var result = await _categoryService.CreateAsync(request);

        if (!result.Succeded)  // Note: Succeded (dos 'c')
            return BadRequest(result.Message);  // Usa Message, no Errors

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryResponse>> Update(int id, UpdateCategoryRequest request)
    {
        var result = await _categoryService.UpdateAsync(id, request);

        if (!result.Succeded)  // Note: Succeded (dos 'c')
        {
            if (result.StatusCode == 404)
                return NotFound(result.Message);

            return BadRequest(result.Message);  // Usa Message, no Errors
        }

        return Ok(result.Data);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoryService.DeleteAsync(id);

        if (!result.Succeded)  // Note: Succeded (dos 'c')
        {
            if (result.StatusCode == 404)
                return NotFound(result.Message);

            return BadRequest(result.Message);  // Usa Message, no Errors
        }

        return NoContent();
    }
}