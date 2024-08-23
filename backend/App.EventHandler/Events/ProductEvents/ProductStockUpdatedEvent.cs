namespace EventHandler.Events.ProductEvents;

public class ProductStockUpdatedEvent : BaseEvent
{
    public int ProductId { get; set; }
    public int NewStockQuantity { get; set; }
}