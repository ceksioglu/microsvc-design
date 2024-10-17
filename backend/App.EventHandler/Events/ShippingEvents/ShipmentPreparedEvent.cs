using EventHandler.Handlers.abstracts;

namespace EventHandler.Events.ShippingEvents;

public class OrderShippedEvent : IEvent
{
    public int OrderId { get; set; }
    public string TrackingNumber { get; set; }
    public DateTime ShippedAt { get; set; }
    public string ShippingProvider { get; set; }
    public Guid Id { get; }
    public DateTime Timestamp { get; }
}