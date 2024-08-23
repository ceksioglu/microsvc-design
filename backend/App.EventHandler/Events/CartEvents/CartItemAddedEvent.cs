namespace EventHandler.Events.CartEvents;

public class CartItemAddedEvent : BaseEvent
{
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}