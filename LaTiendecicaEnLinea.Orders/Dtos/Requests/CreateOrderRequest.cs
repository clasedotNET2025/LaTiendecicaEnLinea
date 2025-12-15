using System.ComponentModel.DataAnnotations;

namespace LaTiendecicaEnLinea.Orders.DTOs.Requests;

public class CreateOrderRequest
{
    [Required, MinLength(1)]
    public List<OrderItemRequest> Items { get; set; } = new();

    [StringLength(500)]
    public string? Notes { get; set; }
}

public class OrderItemRequest
{
    [Required]
    public int ProductId { get; set; }

    [Required, Range(1, 100)]
    public int Quantity { get; set; }
}