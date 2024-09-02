using Core.RabbitMQ.abstracts;
using EventHandler.Handlers.abstracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace EventHandler.Services
{
    public class EventProcessingService : BackgroundService
    {
        private readonly IRabbitMQService _rabbitMQService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventProcessingService> _logger;

        public EventProcessingService(
            IRabbitMQService rabbitMQService,
            IServiceProvider serviceProvider,
            ILogger<EventProcessingService> logger)
        {
            _rabbitMQService = rabbitMQService;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Event Processing Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _rabbitMQService.ConsumeMessage<string>("event_queue", async (message) =>
                    {
                        await ProcessEventMessage(message);
                    });

                    await Task.Delay(1000, stoppingToken); // Wait for 1 second before next iteration
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing events.");
                    await Task.Delay(5000, stoppingToken); // Wait for 5 seconds before retrying
                }
            }

            _logger.LogInformation("Event Processing Service is stopping.");
        }

        private async Task ProcessEventMessage(string message)
        {
            _logger.LogInformation($"Processing event message: {message}");

            // Deserialize the message to determine the event type
            var eventType = DetermineEventType(message);

            if (eventType != null)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
                    var handler = scope.ServiceProvider.GetService(handlerType);

                    if (handler != null)
                    {
                        var eventObject = JsonConvert.DeserializeObject(message, eventType);
                        await (Task)handlerType.GetMethod("HandleAsync").Invoke(handler, new[] { eventObject });
                    }
                    else
                    {
                        _logger.LogWarning($"No handler registered for event type {eventType.Name}");
                    }
                }
            }
            else
            {
                _logger.LogWarning($"Unable to determine event type for message: {message}");
            }
        }

        private Type DetermineEventType(string message)
        {
            var eventData = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);
            if (eventData.ContainsKey("EventType"))
            {
                var eventTypeName = eventData["EventType"].ToString();
                return Type.GetType(eventTypeName);
            }
            return null;
        }
    }
}