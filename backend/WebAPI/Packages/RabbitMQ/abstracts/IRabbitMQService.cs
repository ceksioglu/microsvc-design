using System;
using System.Threading.Tasks;

namespace WebAPI.Packages.RabbitMQ.abstracts
{
    public interface IRabbitMQService
    {
        void PublishMessage<T>(string exchange, string routingKey, T message, string exchangeType = "direct");
        void ConsumeMessage<T>(string queueName, Func<T, Task> messageHandler);
        void BindQueue(string queueName, string exchange, string routingKey);
        void DeclareExchange(string exchangeName, string exchangeType = "direct");
        void DeclareQueue(string queueName);
    }
}