using EventHandler.Handlers.abstracts;

namespace EventHandler.Events;

public abstract class BaseEvent : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}