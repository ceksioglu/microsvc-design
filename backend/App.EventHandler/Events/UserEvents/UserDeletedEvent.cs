namespace EventHandler.Events.UserEvents;

public class UserDeletedEvent : BaseEvent
{
    public int UserId { get; set; }
}