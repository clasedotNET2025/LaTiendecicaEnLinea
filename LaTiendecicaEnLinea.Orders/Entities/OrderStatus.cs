namespace LaTiendecicaEnLinea.Orders.Entities;

public enum OrderStatus
{
    Pending = 1,
    Confirmed = 2,
    Processing = 3,
    Shipped = 4,
    Delivered = 5,
    Cancelled = 10,
    Refunded = 11
}