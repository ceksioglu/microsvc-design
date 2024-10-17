using Core.Mediator.abstracts;
using Core.RabbitMQ.abstracts;

namespace Core.Mediator.concretes
{
    public class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IRabbitMQService _rabbitMQService;

        public Mediator(IServiceProvider serviceProvider, IRabbitMQService rabbitMQService)
        {
            _serviceProvider = serviceProvider;
            _rabbitMQService = rabbitMQService;
        }

        public async Task PublishEvent<TEvent>(TEvent @event) where TEvent : IEvent
        {
            var eventType = @event.GetType().Name;
            var message = System.Text.Json.JsonSerializer.Serialize(@event);
            await _rabbitMQService.PublishMessage(exchange: eventType, routingKey: "event", message);
        }

        public async Task SendCommand<TCommand>(TCommand command) where TCommand : ICommand
        {
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(typeof(TCommand));
            var handler = _serviceProvider.GetService(handlerType) as ICommandHandler<TCommand>;
            if (handler != null)
            {
                await handler.HandleAsync(command);
            }
            else
            {
                throw new InvalidOperationException($"No handler registered for {typeof(TCommand).Name}");
            }
        }
    }
}