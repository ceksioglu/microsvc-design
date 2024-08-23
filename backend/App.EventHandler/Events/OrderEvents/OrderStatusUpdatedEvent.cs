namespace EventHandler.Events.OrderEvents;

public class OrderStatusUpdatedEvent : BaseEvent
{
    public int OrderId { get; set; }
    public string NewStatus { get; set; }
}