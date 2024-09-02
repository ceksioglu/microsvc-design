namespace EventHandler.Events.OrderEvents;

public class OrderCreatedEvent : BaseEvent
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
}