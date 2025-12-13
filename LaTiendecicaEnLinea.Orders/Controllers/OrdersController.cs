using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LaTiendecicaEnLinea.Orders.DTOs.Requests;
using LaTiendecicaEnLinea.Orders.DTOs.Responses;
using LaTiendecicaEnLinea.Orders.Services;

namespace LaTiendecicaEnLinea.Orders.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Customer")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var order = await _orderService.CreateOrderAsync(userId.Value, request);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando pedido");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderSummaryResponse>>> GetMyOrders()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var orders = await _orderService.GetUserOrdersAsync(userId.Value);
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponse>> GetOrder(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var order = await _orderService.GetOrderAsync(id, userId.Value);
        if (order == null) return NotFound();

        return Ok(order);
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var cancelled = await _orderService.CancelOrderAsync(id, userId.Value);
        if (!cancelled) return BadRequest("No se puede cancelar el pedido");

        return NoContent();
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}