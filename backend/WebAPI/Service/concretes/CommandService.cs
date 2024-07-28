using System.Text;
using Newtonsoft.Json;
using Npgsql;
using RabbitMQ.Client;
using WebAPI.Models;
using WebAPI.Service.abstracts;

namespace WebAPI.Service.concretes
{
    /// <summary>
    /// Implements the ICommandService interface for handling write operations.
    /// </summary>
    public class CommandService : ICommandService
    {
        private readonly string _connectionString;
        private readonly ConnectionFactory _rabbitMQFactory;

        public CommandService(string connectionString, ConnectionFactory rabbitMQFactory)
        {
            _connectionString = connectionString;
            _rabbitMQFactory = rabbitMQFactory;
        }

        public async Task<bool> CreateOrderAsync(CreateOrderRequest request)
        {
            // Write to PostgreSQL
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var cmd = new NpgsqlCommand("INSERT INTO orders (data) VALUES (@data)", connection))
                {
                    cmd.Parameters.AddWithValue("data", JsonConvert.SerializeObject(request));
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // Publish event to RabbitMQ
            using (var connection = _rabbitMQFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "order_created",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));

                channel.BasicPublish(exchange: "",
                                     routingKey: "order_created",
                                     basicProperties: null,
                                     body: body);
            }

            return true;
        }

        public async Task<bool> SubmitFeedbackAsync(SubmitFeedbackRequest request)
        {
            // Write to PostgreSQL
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var cmd = new NpgsqlCommand("INSERT INTO feedback (data) VALUES (@data)", connection))
                {
                    cmd.Parameters.AddWithValue("data", JsonConvert.SerializeObject(request));
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // Publish event to RabbitMQ
            using (var connection = _rabbitMQFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "feedback_submitted",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));

                channel.BasicPublish(exchange: "",
                                     routingKey: "feedback_submitted",
                                     basicProperties: null,
                                     body: body);
            }

            return true;
        }
    }
}