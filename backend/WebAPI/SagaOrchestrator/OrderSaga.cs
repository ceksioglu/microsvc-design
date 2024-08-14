using System;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;

namespace WebAPI.SagaOrchestrator
{
    public class OrderSaga
    {
        private readonly IModel _channel;

        public OrderSaga(IConnection connection)
        {
            _channel = connection.CreateModel();
            _channel.QueueDeclare("order_saga_queue", durable: true, exclusive: false, autoDelete: false);
        }

        public async Task StartSaga(string orderId)
        {
            // Step 1: Reserve Inventory
            await PublishCommand("reserve_inventory", new { OrderId = orderId });

            // Step 2: Process Payment
            await PublishCommand("process_payment", new { OrderId = orderId });

            // Step 3: Update Order Status
            await PublishCommand("update_order_status", new { OrderId = orderId, Status = "Completed" });
        }

        private Task PublishCommand(string commandType, object payload)
        {
            var message = JsonConvert.SerializeObject(new { Type = commandType, Payload = payload });
            var body = System.Text.Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "", routingKey: "order_saga_queue", body: body);
            return Task.CompletedTask;
        }

        public void StartListening()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = System.Text.Encoding.UTF8.GetString(body);
                var command = JsonConvert.DeserializeAnonymousType(message, new { Type = "", Payload = new { } });

                switch (command.Type)
                {
                    case "inventory_reserved":
                        // Handle successful inventory reservation
                        break;
                    case "payment_processed":
                        // Handle successful payment processing
                        break;
                    case "order_status_updated":
                        // Handle successful order status update
                        break;
                    case "compensation_needed":
                        // Handle compensation logic (e.g., rollback inventory, refund payment)
                        break;
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(queue: "order_saga_queue", autoAck: false, consumer: consumer);
        }
    }
}