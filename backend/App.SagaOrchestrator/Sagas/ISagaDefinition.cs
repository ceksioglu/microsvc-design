using EventHandler.Handlers.abstracts;

namespace SagaOrchestrator.Sagas;

public interface ISagaDefinition
{
    Type StartEventType { get; }
    bool CanHandle(IEvent @event);
    Task StartAsync(IEvent @event, IEventPublisher eventPublisher);
    Task HandleAsync(IEvent @event, IEventPublisher eventPublisher);
}