namespace Core.Mediator.abstracts
{
    public interface IMediator
    {
        Task PublishEvent<TEvent>(TEvent @event) where TEvent : IEvent;
        Task SendCommand<TCommand>(TCommand command) where TCommand : ICommand;
    }
}