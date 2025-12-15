using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaTiendecicaEnLinea.Orders.Entities;

public class OrderItem
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required, MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Required, Range(1, 999)]
    public int Quantity { get; set; }

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}