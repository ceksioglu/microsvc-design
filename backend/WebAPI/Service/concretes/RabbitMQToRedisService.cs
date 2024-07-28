using WebAPI.Service.abstracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System;
using System.Text;
using System.Text.Json;
using WebAPI.Models;

namespace WebAPI.Service.concretes
{
    public class RabbitMQToRedisService : IRabbitMQToRedisService
    {
        private readonly ConnectionFactory _rabbitFactory;
        private readonly IConnection _rabbitConnection;
        private readonly IModel _channel;
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly IDatabase _redisDb;

        public RabbitMQToRedisService(ConnectionFactory rabbitFactory, IConnectionMultiplexer redisConnection)
        {
            _rabbitFactory = rabbitFactory ?? throw new ArgumentNullException(nameof(rabbitFactory));
            _redisConnection = redisConnection ?? throw new ArgumentNullException(nameof(redisConnection));

            _rabbitConnection = _rabbitFactory.CreateConnection();
            _channel = _rabbitConnection.CreateModel();
            _redisDb = _redisConnection.GetDatabase();
        }

        public void StartListening()
        {
            ListenToQueue("order_created", ProcessOrderCreated);
            ListenToQueue("feedback_submitted", ProcessFeedbackSubmitted);
        }

        private void ListenToQueue(string queueName, Action<string> processMessage)
        {
            _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                
                processMessage(message);
            };

            _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        }

        private void ProcessOrderCreated(string message)
        {
            try
            {
                var order = JsonSerializer.Deserialize<WebAPI.Models.Order>(message);
                if (order != null)
                {
                    UpdateUserDashboard(order);
                    UpdateInventoryStatus(order);
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error deserializing order message: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing order message: {ex.Message}");
            }
        }

        private void ProcessFeedbackSubmitted(string message)
        {
            try
            {
                var feedback = JsonSerializer.Deserialize<Feedback>(message);
                if (feedback != null)
                {
                    var serializedFeedback = JsonSerializer.Serialize(feedback);
                    _redisDb.ListRightPush("recent_feedback", serializedFeedback);
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error deserializing feedback message: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing feedback message: {ex.Message}");
            }
        }

        private void UpdateUserDashboard(WebAPI.Models.Order order)
        {
            var dashboardKey = $"user_dashboard:{order.Id}"; // Assuming Order.Id is the user ID
            var dashboardJson = _redisDb.StringGet(dashboardKey);
            
            DashboardData dashboard;
            if (dashboardJson.IsNull)
            {
                dashboard = new DashboardData
                {
                    RecentOrders = new List<OrderSummary>(),
                    TotalOrders = 0,
                    TotalSpent = 0
                };
            }
            else
            {
                dashboard = JsonSerializer.Deserialize<DashboardData>(dashboardJson);
            }

            dashboard.RecentOrders.Insert(0, new OrderSummary
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status
            });
            dashboard.RecentOrders = dashboard.RecentOrders.Take(5).ToList();
            dashboard.TotalOrders++;
            dashboard.TotalSpent += order.TotalAmount;

            var updatedDashboardJson = JsonSerializer.Serialize(dashboard);
            _redisDb.StringSet(dashboardKey, updatedDashboardJson);
        }

        private void UpdateInventoryStatus(WebAPI.Models.Order order)
        {
            var inventoryJson = _redisDb.StringGet("inventory_status");
            var inventory = JsonSerializer.Deserialize<List<Product>>(inventoryJson);

            foreach (var item in order.Items)
            {
                var product = inventory.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                }
            }

            var updatedInventoryJson = JsonSerializer.Serialize(inventory);
            _redisDb.StringSet("inventory_status", updatedInventoryJson);
        }
        public void Dispose()
        {
            _channel?.Dispose();
            _rabbitConnection?.Dispose();
        }
    }
}