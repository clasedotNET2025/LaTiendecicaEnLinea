using LaTiendecicaEnLinea.Catalog.Dtos.Category;
using LaTiendecicaEnLinea.Catalog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaTiendecicaEnLinea.Catalog.Controllers;

/// <summary>
/// Controller for managing product categories
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize] // Require authentication by default for all endpoints
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    /// <summary>
    /// Initializes a new instance of the CategoriesController class
    /// </summary>
    /// <param name="categoryService">Service for category operations</param>
    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Retrieves all categories
    /// </summary>
    /// <returns>List of all categories</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetAll()
    {
        var result = await _categoryService.GetAllAsync();
        return Ok(result.Data);
    }

    /// <summary>
    /// Retrieves a specific category by ID
    /// </summary>
    /// <param name="id">Category identifier</param>
    /// <returns>The requested category</returns>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponse>> GetById(int id)
    {
        var result = await _categoryService.GetByIdAsync(id);

        if (!result.Succeded)
            return NotFound(result.Message);

        return Ok(result.Data);
    }

    /// <summary>
    /// Creates a new category
    /// </summary>
    /// <param name="request">Category creation data</param>
    /// <returns>The newly created category</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")] // Only users with Admin role can create categories
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CategoryResponse>> Create(CreateCategoryRequest request)
    {
        var result = await _categoryService.CreateAsync(request);

        if (!result.Succeded)
            return BadRequest(result.Message);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Updates an existing category
    /// </summary>
    /// <param name="id">Category identifier</param>
    /// <param name="request">Category update data</param>
    /// <returns>The updated category</returns>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")] // Only users with Admin role can update categories
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CategoryResponse>> Update(int id, UpdateCategoryRequest request)
    {
        var result = await _categoryService.UpdateAsync(id, request);

        if (!result.Succeded)
        {
            if (result.StatusCode == 404)
                return NotFound(result.Message);

            return BadRequest(result.Message);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Deletes a category
    /// </summary>
    /// <param name="id">Category identifier</param>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")] // Only users with Admin role can delete categories
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoryService.DeleteAsync(id);

        if (!result.Succeded)
        {
            if (result.StatusCode == 404)
                return NotFound(result.Message);

            return BadRequest(result.Message);
        }

        return NoContent();
    }
}