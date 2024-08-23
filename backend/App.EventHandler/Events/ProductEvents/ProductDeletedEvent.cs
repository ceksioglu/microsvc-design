namespace EventHandler.Events.ProductEvents;

public class ProductDeletedEvent : BaseEvent
{
    public int ProductId { get; set; }
}