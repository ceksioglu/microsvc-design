namespace EventHandler.Events.CartEvents;

public class CartItemRemovedEvent : BaseEvent
{
    public int UserId { get; set; }
    public int ProductId { get; set; }
}