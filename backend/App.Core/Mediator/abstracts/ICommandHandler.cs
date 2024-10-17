using Core.Mediator.abstracts;

namespace Core.Mediator.concretes;

public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    Task HandleAsync(TCommand command);
}