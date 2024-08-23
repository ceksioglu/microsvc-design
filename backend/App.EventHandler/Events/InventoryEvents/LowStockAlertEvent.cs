namespace EventHandler.Events.InventoryEvents;

public class LowStockAlertEvent : BaseEvent
{
    public int ProductId { get; set; }
    public int CurrentStock { get; set; }
    public int ThresholdLevel { get; set; }
}