using LaTiendecicaEnLinea.Orders.DTOs.Requests;
using LaTiendecicaEnLinea.Orders.DTOs.Responses;
using LaTiendecicaEnLinea.Orders.Services;
using LaTiendecicaEnLinea.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LaTiendecicaEnLinea.Orders.Controllers;

/// <summary>
/// Controller for managing orders
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize] // Require authentication by default for all endpoints
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    /// <summary>
    /// Initializes a new instance of the OrdersController class
    /// </summary>
    /// <param name="orderService">Service for order operations</param>
    /// <param name="logger">Logger instance</param>
    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new order
    /// </summary>
    /// <param name="request">Order creation data</param>
    /// <returns>The newly created order</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Customer")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _orderService.CreateOrderAsync(userId.Value, request);

        if (!result.Succeded)
            return BadRequest(result.Message);

        return CreatedAtAction(nameof(GetOrder), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Retrieves all orders for the current user
    /// </summary>
    /// <returns>List of user's orders</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Customer")]
    [ProducesResponseType(typeof(IEnumerable<OrderSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderSummaryResponse>>> GetMyOrders()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _orderService.GetUserOrdersAsync(userId.Value);

        if (!result.Succeded)
            return StatusCode(result.StatusCode, result.Message);

        return Ok(result.Data);
    }

    /// <summary>
    /// Retrieves a specific order by ID
    /// </summary>
    /// <param name="id">Order identifier</param>
    /// <returns>The requested order</returns>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Customer")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetOrder(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _orderService.GetOrderAsync(id, userId.Value);

        if (!result.Succeded)
            return NotFound(result.Message);

        return Ok(result.Data);
    }

    /// <summary>
    /// Cancels an existing order
    /// </summary>
    /// <param name="id">Order identifier</param>
    /// <returns>No content if successful</returns>
    [HttpPut("{id:guid}/cancel")]
    [Authorize(Roles = "Admin,Customer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _orderService.CancelOrderAsync(id, userId.Value);

        if (!result.Succeded)
            return BadRequest(result.Message);

        return NoContent();
    }

    private Guid? GetUserId()
    {
        // Identity usa ClaimTypes.NameIdentifier
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value
                       ?? User.FindFirst("userId")?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        // Si es un string simple de Identity (no GUID)
        // Identity devuelve string, no GUID
        // Podrías guardarlo como string en la BD
        // O si realmente necesitas GUID:
        return string.IsNullOrEmpty(userIdClaim)
            ? null
            : new Guid(userIdClaim);
    }
}