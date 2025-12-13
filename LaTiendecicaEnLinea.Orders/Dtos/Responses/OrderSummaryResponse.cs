namespace LaTiendecicaEnLinea.Orders.DTOs.Responses;

public class OrderSummaryResponse
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public int ItemCount { get; set; }
}