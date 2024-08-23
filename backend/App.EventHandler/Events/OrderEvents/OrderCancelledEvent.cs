namespace EventHandler.Events.OrderEvents;

public class OrderCancelledEvent : BaseEvent
{
    public int OrderId { get; set; }
}