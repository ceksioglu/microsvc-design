using System.Text;
using Core.RabbitMQ.abstracts;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Core.RabbitMQ.concretes
{
    public class RabbitMQService : IRabbitMQService, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQService(IConnection connection)
        {
            _connection = connection;
            _channel = _connection.CreateModel();
        }

        public Task PublishMessage<T>(string exchange, string routingKey, T message, string exchangeType = ExchangeType.Direct)
        {
            return Task.Run(() =>
            {
                _channel.ExchangeDeclare(exchange, exchangeType, durable: true, autoDelete: false);

                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;

                _channel.BasicPublish(exchange: exchange,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body);
            });
        }

        public void ConsumeMessage<T>(string queueName, Func<T, Task> messageHandler)
        {
            _channel.QueueDeclare(queue: queueName,
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(body));

                try
                {
                    await messageHandler(message);
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception)
                {
                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsume(queue: queueName,
                                  autoAck: false,
                                  consumer: consumer);
        }

        public void BindQueue(string queueName, string exchange, string routingKey)
        {
            _channel.QueueBind(queue: queueName,
                               exchange: exchange,
                               routingKey: routingKey);
        }

        public void DeclareExchange(string exchangeName, string exchangeType = ExchangeType.Direct)
        {
            _channel.ExchangeDeclare(exchange: exchangeName,
                                     type: exchangeType,
                                     durable: true,
                                     autoDelete: false);
        }

        public void DeclareQueue(string queueName)
        {
            _channel.QueueDeclare(queue: queueName,
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}