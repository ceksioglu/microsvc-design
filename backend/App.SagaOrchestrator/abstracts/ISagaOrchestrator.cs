using EventHandler.Handlers.abstracts;

namespace SagaOrchestrator.abstracts;

public interface ISagaOrchestrator
{ 
    Task ProcessEventAsync<TEvent>(TEvent @event) where TEvent : IEvent;
}