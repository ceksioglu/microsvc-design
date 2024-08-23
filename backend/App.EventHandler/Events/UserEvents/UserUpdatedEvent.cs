namespace EventHandler.Events.UserEvents;

public class UserUpdatedEvent : BaseEvent
{
    public int UserId { get; set; }
    public string Email { get; set; }
}