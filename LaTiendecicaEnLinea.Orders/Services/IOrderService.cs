using LaTiendecicaEnLinea.Orders.DTOs.Requests;
using LaTiendecicaEnLinea.Orders.DTOs.Responses;

namespace LaTiendecicaEnLinea.Orders.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(Guid userId, CreateOrderRequest request);
    Task<OrderResponse?> GetOrderAsync(Guid orderId, Guid userId);
    Task<List<OrderSummaryResponse>> GetUserOrdersAsync(Guid userId);
    Task<OrderResponse> UpdateOrderStatusAsync(Guid orderId, string newStatus, string? adminNotes);
    Task<List<OrderResponse>> GetAllOrdersAsync(string? statusFilter);
    Task<bool> CancelOrderAsync(Guid orderId, Guid userId);
}