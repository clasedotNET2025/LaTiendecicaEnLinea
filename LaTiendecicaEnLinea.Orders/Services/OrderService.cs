using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using LaTiendecicaEnLinea.Orders.Data;
using LaTiendecicaEnLinea.Orders.Entities;
using LaTiendecicaEnLinea.Orders.DTOs.Requests;
using LaTiendecicaEnLinea.Orders.DTOs.Responses;
using LaTiendecicaEnLinea.Shared.Common;

namespace LaTiendecicaEnLinea.Orders.Services;

/// <summary>
/// Service for order operations
/// </summary>
public class OrderService : IOrderService
{
    private readonly OrdersDbContext _context;
    private readonly ILogger<OrderService> _logger;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the OrderService class
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="httpClientFactory">HTTP client factory</param>
    public OrderService(
        OrdersDbContext context,
        ILogger<OrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new order
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="request">Order creation data</param>
    /// <returns>Service result with created order</returns>
    /// <summary>
    /// Creates a new order
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="request">Order creation data</param>
    /// <returns>Service result with created order</returns>
    public async Task<ServiceResult<OrderResponse>> CreateOrderAsync(Guid userId, CreateOrderRequest request)
    {
        try
        {
            var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 6)}";

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = orderNumber,
                UserId = userId,
                Status = OrderStatus.Pending,
                CustomerNotes = request.Notes,
                OrderDate = DateTime.UtcNow,
                OrderItems = new List<OrderItem>()
            };

            decimal totalAmount = 0;

            foreach (var itemRequest in request.Items)
            {
                // EN LUGAR de llamar a Catalog API, asume que el producto existe
                // O mejor, verifica en tu propia BD si Orders tiene productos
                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = itemRequest.ProductId,
                    ProductName = $"Producto {itemRequest.ProductId}", // Nombre temporal
                    UnitPrice = 10.0m, // Precio temporal
                    Quantity = itemRequest.Quantity,
                    Subtotal = 10.0m * itemRequest.Quantity,
                    CreatedAt = DateTime.UtcNow
                };

                order.OrderItems.Add(orderItem);
                totalAmount += orderItem.Subtotal;
            }

            order.TotalAmount = totalAmount;

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Order created: {OrderNumber} by user {UserId}", order.OrderNumber, userId);

            var response = MapToOrderResponse(order);
            return ServiceResult<OrderResponse>.Success(response, "Order created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for user {UserId}", userId);
            return ServiceResult<OrderResponse>.Failure($"Error creating order: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves an order by identifier for a specific user
    /// </summary>
    /// <param name="orderId">Order identifier</param>
    /// <param name="userId">User identifier</param>
    /// <returns>Service result with order details</returns>
    public async Task<ServiceResult<OrderResponse>> GetOrderAsync(Guid orderId, Guid userId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
        {
            return ServiceResult<OrderResponse>.Failure("Order not found");
        }

        var response = MapToOrderResponse(order);
        return ServiceResult<OrderResponse>.Success(response);
    }

    /// <summary>
    /// Retrieves all orders for a specific user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>Service result with list of user orders</returns>
    public async Task<ServiceResult<IEnumerable<OrderSummaryResponse>>> GetUserOrdersAsync(Guid userId)
    {
        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        var response = orders.Select(MapToOrderSummaryResponse).ToList();
        return ServiceResult<IEnumerable<OrderSummaryResponse>>.Success(response);
    }

    /// <summary>
    /// Updates order status (admin only)
    /// </summary>
    /// <param name="orderId">Order identifier</param>
    /// <param name="newStatus">New order status</param>
    /// <param name="adminNotes">Admin notes</param>
    /// <returns>Service result with updated order</returns>
    public async Task<ServiceResult<OrderResponse>> UpdateOrderStatusAsync(Guid orderId, string newStatus, string? adminNotes)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
        {
            return ServiceResult<OrderResponse>.Failure("Order not found");
        }

        if (!Enum.TryParse<OrderStatus>(newStatus, out var status))
        {
            return ServiceResult<OrderResponse>.Failure("Invalid order status");
        }

        var oldStatus = order.Status.ToString();
        order.Status = status;
        order.AdminNotes = adminNotes;
        order.UpdateTimestamp();

        await _context.SaveChangesAsync();

        _logger.LogInformation("Order status updated: {OrderId} from {OldStatus} to {NewStatus}",
            orderId, oldStatus, newStatus);

        var response = MapToOrderResponse(order);
        return ServiceResult<OrderResponse>.Success(response, "Order status updated successfully");
    }

    /// <summary>
    /// Retrieves all orders with optional status filtering (admin only)
    /// </summary>
    /// <param name="statusFilter">Optional status filter</param>
    /// <returns>Service result with list of all orders</returns>
    public async Task<ServiceResult<IEnumerable<OrderResponse>>> GetAllOrdersAsync(string? statusFilter)
    {
        var query = _context.Orders.Include(o => o.OrderItems).AsQueryable();

        if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<OrderStatus>(statusFilter, out var status))
        {
            query = query.Where(o => o.Status == status);
        }

        var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
        var response = orders.Select(MapToOrderResponse).ToList();
        return ServiceResult<IEnumerable<OrderResponse>>.Success(response);
    }

    /// <summary>
    /// Cancels an order
    /// </summary>
    /// <param name="orderId">Order identifier</param>
    /// <param name="userId">User identifier</param>
    /// <returns>Service result indicating operation status</returns>
    public async Task<ServiceResult> CancelOrderAsync(Guid orderId, Guid userId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null || order.UserId != userId)
        {
            return ServiceResult.Failure("Order not found");
        }

        if (order.Status != OrderStatus.Pending)
        {
            return ServiceResult.Failure("Only pending orders can be cancelled");
        }

        var oldStatus = order.Status.ToString();
        order.Status = OrderStatus.Cancelled;
        order.UpdateTimestamp();

        await _context.SaveChangesAsync();

        _logger.LogInformation("Order cancelled: {OrderId} by user {UserId}", orderId, userId);
        return ServiceResult.Success("Order cancelled successfully");
    }

    private OrderResponse MapToOrderResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            OrderDate = order.OrderDate,
            UpdatedAt = order.UpdatedAt,
            CustomerNotes = order.CustomerNotes,
            AdminNotes = order.AdminNotes,
            Items = order.OrderItems.Select(oi => new OrderItemResponse
            {
                ProductId = oi.ProductId,
                ProductName = oi.ProductName,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                Subtotal = oi.Subtotal
            }).ToList()
        };
    }

    private OrderSummaryResponse MapToOrderSummaryResponse(Order order)
    {
        return new OrderSummaryResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            OrderDate = order.OrderDate,
            ItemCount = order.OrderItems.Count
        };
    }

    private class ProductData
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}