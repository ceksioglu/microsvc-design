using EventHandler.Handlers.abstracts;
using Microsoft.Extensions.Logging;

namespace EventHandler.Handlers.concretes
{
    public class GenericEventHandler<TEvent> : IEventHandler<TEvent> where TEvent : IEvent
    {
        private readonly ILogger<GenericEventHandler<TEvent>> _logger;

        public GenericEventHandler(ILogger<GenericEventHandler<TEvent>> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(TEvent @event)
        {
            _logger.LogInformation($"Handling event of type {typeof(TEvent).Name}. Event details: {@event}");
            return Task.CompletedTask;
        }
    }
}