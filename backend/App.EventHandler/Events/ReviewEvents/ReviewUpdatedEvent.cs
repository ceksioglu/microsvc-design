namespace EventHandler.Events.ReviewEvents;

public class ReviewUpdatedEvent : BaseEvent
{
    public int ReviewId { get; set; }
    public int Rating { get; set; }
}