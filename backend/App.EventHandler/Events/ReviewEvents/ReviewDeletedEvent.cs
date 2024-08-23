namespace EventHandler.Events.ReviewEvents;

public class ReviewDeletedEvent : BaseEvent
{
    public int ReviewId { get; set; }
    public int ProductId { get; set; }
}