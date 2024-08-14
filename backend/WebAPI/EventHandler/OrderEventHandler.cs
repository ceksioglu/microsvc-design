using System;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace WebAPI.EventHandler
{
    public class OrderEventHandler
    {
        private readonly IModel _channel;
        private readonly IDatabase _redisDb;

        public OrderEventHandler(IConnection rabbitConnection, IConnectionMultiplexer redisConnection)
        {
            _channel = rabbitConnection.CreateModel();
            _channel.QueueDeclare("order_events", durable: true, exclusive: false, autoDelete: false);
            _redisDb = redisConnection.GetDatabase();
        }

        public void StartListening()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = System.Text.Encoding.UTF8.GetString(body);
                var orderEvent = JsonConvert.DeserializeAnonymousType(message, new { Type = "", Payload = new { OrderId = "", Status = "" } });

                switch (orderEvent.Type)
                {
                    case "order_created":
                    case "order_updated":
                    case "order_completed":
                        await UpdateOrderInRedis(orderEvent.Payload.OrderId, orderEvent.Payload.Status);
                        break;
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(queue: "order_events", autoAck: false, consumer: consumer);
        }

        private async Task UpdateOrderInRedis(string orderId, string status)
        {
            await _redisDb.HashSetAsync($"order:{orderId}", new HashEntry[]
            {
                new HashEntry("status", status),
                new HashEntry("last_updated", DateTime.UtcNow.ToString())
            });
        }
    }
}