using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaTiendecicaEnLinea.Orders.Entities;

public class Order
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(20)]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [MaxLength(500)]
    public string? ShippingAddress { get; set; }

    [MaxLength(100)]
    public string? ShippingCity { get; set; }

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [MaxLength(1000)]
    public string? CustomerNotes { get; set; }

    [MaxLength(1000)]
    public string? AdminNotes { get; set; }

    [Required]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public void CalculateTotal()
    {
        TotalAmount = OrderItems.Sum(item => item.Subtotal);
    }

    public void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}