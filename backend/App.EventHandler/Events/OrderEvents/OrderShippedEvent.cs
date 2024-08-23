namespace EventHandler.Events.OrderEvents;

public class OrderShippedEvent : BaseEvent
{ 
    public int OrderId { get; set; }
    public string TrackingNumber { get; set; } 
    public DateTime EstimatedDeliveryDate { get; set; }
}