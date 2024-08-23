namespace EventHandler.Events.CartEvents;

public class CartClearedEvent : BaseEvent
{
    public int UserId { get; set; }
}