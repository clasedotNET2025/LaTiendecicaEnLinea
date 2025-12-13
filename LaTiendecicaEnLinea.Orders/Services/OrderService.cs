using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using LaTiendecicaEnLinea.Orders.Data;
using LaTiendecicaEnLinea.Orders.Entities;
using LaTiendecicaEnLinea.Orders.DTOs.Requests;
using LaTiendecicaEnLinea.Orders.DTOs.Responses;

namespace LaTiendecicaEnLinea.Orders.Services;

public class OrderService : IOrderService
{
    private readonly OrdersDbContext _context;
    private readonly ILogger<OrderService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IPublishEndpoint _publishEndpoint;

    public OrderService(
        OrdersDbContext context,
        ILogger<OrderService> logger,
        IHttpClientFactory httpClientFactory,
        IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("catalog");
        _publishEndpoint = publishEndpoint;
    }

    public async Task<OrderResponse> CreateOrderAsync(Guid userId, CreateOrderRequest request)
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
            var productResponse = await _httpClient.GetAsync($"api/products/{itemRequest.ProductId}");
            if (!productResponse.IsSuccessStatusCode)
                throw new Exception($"Producto {itemRequest.ProductId} no encontrado");

            var productJson = await productResponse.Content.ReadAsStringAsync();
            var productData = JsonSerializer.Deserialize<ProductData>(productJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = itemRequest.ProductId,
                ProductName = productData?.Name ?? "Producto",
                UnitPrice = productData?.Price ?? 0,
                Quantity = itemRequest.Quantity,
                Subtotal = (productData?.Price ?? 0) * itemRequest.Quantity,
                CreatedAt = DateTime.UtcNow
            };

            order.OrderItems.Add(orderItem);
            totalAmount += orderItem.Subtotal;
        }

        order.TotalAmount = totalAmount;

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        await _publishEndpoint.Publish(new OrderCreatedEvent
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            TotalAmount = order.TotalAmount,
            CreatedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Pedido creado: {OrderNumber}", order.OrderNumber);

        return MapToOrderResponse(order);
    }

    public async Task<OrderResponse?> GetOrderAsync(Guid orderId, Guid userId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        return order != null ? MapToOrderResponse(order) : null;
    }

    public async Task<List<OrderSummaryResponse>> GetUserOrdersAsync(Guid userId)
    {
        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return orders.Select(MapToOrderSummaryResponse).ToList();
    }

    public async Task<OrderResponse> UpdateOrderStatusAsync(Guid orderId, string newStatus, string? adminNotes)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null) throw new KeyNotFoundException("Pedido no encontrado");

        if (!Enum.TryParse<OrderStatus>(newStatus, out var status))
            throw new ArgumentException("Estado no válido");

        var oldStatus = order.Status.ToString();
        order.Status = status;
        order.AdminNotes = adminNotes;
        order.UpdateTimestamp();

        await _context.SaveChangesAsync();

        await _publishEndpoint.Publish(new OrderStatusChangedEvent
        {
            OrderId = order.Id,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedAt = DateTime.UtcNow
        });

        return MapToOrderResponse(order);
    }

    public async Task<List<OrderResponse>> GetAllOrdersAsync(string? statusFilter)
    {
        var query = _context.Orders.Include(o => o.OrderItems).AsQueryable();

        if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<OrderStatus>(statusFilter, out var status))
        {
            query = query.Where(o => o.Status == status);
        }

        var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
        return orders.Select(MapToOrderResponse).ToList();
    }

    public async Task<bool> CancelOrderAsync(Guid orderId, Guid userId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null || order.UserId != userId) return false;

        if (order.Status != OrderStatus.Pending) return false;

        var oldStatus = order.Status.ToString();
        order.Status = OrderStatus.Cancelled;
        order.UpdateTimestamp();

        await _context.SaveChangesAsync();

        await _publishEndpoint.Publish(new OrderStatusChangedEvent
        {
            OrderId = order.Id,
            OldStatus = oldStatus,
            NewStatus = "Cancelled",
            ChangedAt = DateTime.UtcNow
        });

        return true;
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

// Eventos para MassTransit
public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderStatusChangedEvent
{
    public Guid OrderId { get; set; }
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}