namespace EventHandler.Events.UserEvents;

public class UserCreatedEvent : BaseEvent
{
    public int UserId { get; set; }
    public string Email { get; set; }
}