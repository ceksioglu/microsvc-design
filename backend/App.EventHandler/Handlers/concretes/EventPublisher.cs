using Core.RabbitMQ.abstracts;
using EventHandler.Handlers.abstracts;
using Newtonsoft.Json;

namespace EventHandler.Handlers.concretes;

public class EventPublisher : IEventPublisher
{
    private readonly IRabbitMQService _rabbitMQService;

    public EventPublisher(IRabbitMQService rabbitMQService)
    {
        _rabbitMQService = rabbitMQService;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, string exchangeName, string routingKey) where TEvent : IEvent
    {
        var message = JsonConvert.SerializeObject(@event);
        await _rabbitMQService.PublishMessage(exchangeName, routingKey, message);
    }
}