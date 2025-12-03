using MassTransit;

namespace LaTiendecicaEnLinea.Shared
{
    [ExcludeFromTopology]
    public interface IRabbitEvent
    {
        public Guid EventId { get; }
        public DateTime CreatedAt { get; }
    }
}