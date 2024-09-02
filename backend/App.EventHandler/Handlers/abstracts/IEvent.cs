namespace EventHandler.Handlers.abstracts;

public interface IEvent
{
    Guid Id { get; }
    DateTime Timestamp { get; }
}