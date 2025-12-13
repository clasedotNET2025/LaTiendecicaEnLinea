using System.ComponentModel.DataAnnotations;

namespace LaTiendecicaEnLinea.Orders.DTOs.Requests;

public class UpdateOrderStatusRequest
{
    [Required]
    [RegularExpression("Confirmed|Processing|Shipped|Delivered|Cancelled")]
    public string Status { get; set; } = string.Empty;

    [StringLength(250)]
    public string? AdminNotes { get; set; }
}