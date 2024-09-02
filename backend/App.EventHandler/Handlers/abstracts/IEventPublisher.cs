namespace EventHandler.Handlers.abstracts;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, string exchangeName, string routingKey) where TEvent : IEvent;
}