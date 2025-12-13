namespace LaTiendecicaEnLinea.Orders.DTOs.Responses;

public class OrderResponse
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CustomerNotes { get; set; }
    public string? AdminNotes { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
}