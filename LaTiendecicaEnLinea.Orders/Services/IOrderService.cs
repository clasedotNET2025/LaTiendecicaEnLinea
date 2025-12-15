using LaTiendecicaEnLinea.Orders.DTOs.Requests;
using LaTiendecicaEnLinea.Orders.DTOs.Responses;
using LaTiendecicaEnLinea.Shared.Common;

namespace LaTiendecicaEnLinea.Orders.Services;

/// <summary>
/// Interface for order service operations
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Creates a new order
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="request">Order creation data</param>
    /// <returns>Service result with created order</returns>
    Task<ServiceResult<OrderResponse>> CreateOrderAsync(Guid userId, CreateOrderRequest request);

    /// <summary>
    /// Retrieves an order by identifier for a specific user
    /// </summary>
    /// <param name="orderId">Order identifier</param>
    /// <param name="userId">User identifier</param>
    /// <returns>Service result with order details</returns>
    Task<ServiceResult<OrderResponse>> GetOrderAsync(Guid orderId, Guid userId);

    /// <summary>
    /// Retrieves all orders for a specific user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>Service result with list of user orders</returns>
    Task<ServiceResult<IEnumerable<OrderSummaryResponse>>> GetUserOrdersAsync(Guid userId);

    /// <summary>
    /// Updates order status (admin only)
    /// </summary>
    /// <param name="orderId">Order identifier</param>
    /// <param name="newStatus">New order status</param>
    /// <param name="adminNotes">Admin notes</param>
    /// <returns>Service result with updated order</returns>
    Task<ServiceResult<OrderResponse>> UpdateOrderStatusAsync(Guid orderId, string newStatus, string? adminNotes);

    /// <summary>
    /// Retrieves all orders with optional status filtering (admin only)
    /// </summary>
    /// <param name="statusFilter">Optional status filter</param>
    /// <returns>Service result with list of all orders</returns>
    Task<ServiceResult<IEnumerable<OrderResponse>>> GetAllOrdersAsync(string? statusFilter);

    /// <summary>
    /// Cancels an order
    /// </summary>
    /// <param name="orderId">Order identifier</param>
    /// <param name="userId">User identifier</param>
    /// <returns>Service result indicating operation status</returns>
    Task<ServiceResult> CancelOrderAsync(Guid orderId, Guid userId);
}