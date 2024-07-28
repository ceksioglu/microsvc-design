using System.Text;
using Newtonsoft.Json;
using Npgsql;
using RabbitMQ.Client;
using WebAPI.Models;
using WebAPI.Service.abstracts;
using WebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Service.concretes
{
    /// <summary>
    /// Implements the ICommandService interface for handling write operations.
    /// This service interacts with PostgreSQL for data persistence and RabbitMQ for event publishing.
    /// </summary>
    public class CommandService : ICommandService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ConnectionFactory _rabbitMQFactory;

        /// <summary>
        /// Initializes a new instance of the CommandService class.
        /// </summary>
        /// <param name="dbContext">The database context for Entity Framework operations.</param>
        /// <param name="rabbitMQFactory">The RabbitMQ connection factory for message queue operations.</param>
        public CommandService(ApplicationDbContext dbContext, ConnectionFactory rabbitMQFactory)
        {
            _dbContext = dbContext;
            _rabbitMQFactory = rabbitMQFactory;
        }

        /// <summary>
        /// Creates a new order asynchronously.
        /// </summary>
        /// <param name="order">The order to be created.</param>
        /// <returns>The created order with its assigned ID.</returns>
        public async Task<Order> CreateOrderAsync(Order order)
        {
            // Write to PostgreSQL using Entity Framework
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            // Publish event to RabbitMQ
            using (var connection = _rabbitMQFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "order_created",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(order));

                channel.BasicPublish(exchange: "",
                                     routingKey: "order_created",
                                     basicProperties: null,
                                     body: body);
            }

            return order;
        }

        /// <summary>
        /// Submits feedback asynchronously.
        /// </summary>
        /// <param name="feedback">The feedback to be submitted.</param>
        /// <returns>The submitted feedback with its assigned ID.</returns>
        public async Task<Feedback> SubmitFeedbackAsync(Feedback feedback)
        {
            // Write to PostgreSQL using Entity Framework
            _dbContext.Feedbacks.Add(feedback);
            await _dbContext.SaveChangesAsync();

            // Publish event to RabbitMQ
            using (var connection = _rabbitMQFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "feedback_submitted",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(feedback));

                channel.BasicPublish(exchange: "",
                                     routingKey: "feedback_submitted",
                                     basicProperties: null,
                                     body: body);
            }

            return feedback;
        }
    }
}