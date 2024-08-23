namespace EventHandler.Events.ReviewEvents;

public class ReviewCreatedEvent : BaseEvent
{
    public int ReviewId { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Rating { get; set; }
}