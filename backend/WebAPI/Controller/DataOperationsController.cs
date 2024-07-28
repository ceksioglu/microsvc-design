using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.Service.abstracts;
using System.Collections.Generic;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using StackExchange.Redis;

namespace WebAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataOperationsController : ControllerBase
    {
        private readonly IQueryService _queryService;
        private readonly ICommandService _commandService;
        private readonly ConnectionFactory _rabbitMQFactory;
        private readonly IConnectionMultiplexer _redisConnection;

        public DataOperationsController(IQueryService queryService, ICommandService commandService, ConnectionFactory rabbitMQFactory, IConnectionMultiplexer redisConnection)
        {
            _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _rabbitMQFactory = rabbitMQFactory ?? throw new ArgumentNullException(nameof(rabbitMQFactory));
            _redisConnection = redisConnection ?? throw new ArgumentNullException(nameof(redisConnection));
        }

        [HttpGet("userdashboard")]
        [ProducesResponseType(typeof(DashboardData), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetUserDashboardAsync()
        {
            try
            {
                var dashboardData = await _queryService.GetUserDashboardAsync();
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                // TODO: Log the exception
                return StatusCode(500, "An error occurred while fetching the user dashboard.");
            }
        }

        [HttpGet("inventorystatus")]
        [ProducesResponseType(typeof(List<Product>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetInventoryStatusAsync()
        {
            try
            {
                var inventoryData = await _queryService.GetInventoryStatusAsync();
                return Ok(inventoryData);
            }
            catch (Exception ex)
            {
                // TODO: Log the exception
                return StatusCode(500, "An error occurred while fetching the inventory status.");
            }
        }

        [HttpPost("createorder")]
        [ProducesResponseType(typeof(WebAPI.Models.Order), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateOrderAsync([FromBody] WebAPI.Models.Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _commandService.CreateOrderAsync(order);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // TODO: Log the exception
                return StatusCode(500, "An error occurred while creating the order.");
            }
        }

        [HttpPost("submitfeedback")]
        [ProducesResponseType(typeof(Feedback), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SubmitFeedbackAsync([FromBody] Feedback feedback)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _commandService.SubmitFeedbackAsync(feedback);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // TODO: Log the exception
                return StatusCode(500, "An error occurred while submitting the feedback.");
            }
        }

        [HttpPost("test/createorder")]
        [ProducesResponseType(typeof(WebAPI.Models.Order), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public IActionResult TestCreateOrder([FromBody] WebAPI.Models.Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Publish order to RabbitMQ
                using (var connection = _rabbitMQFactory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "order_created", durable: false, exclusive: false, autoDelete: false, arguments: null);

                    var json = JsonSerializer.Serialize(order);
                    var body = Encoding.UTF8.GetBytes(json);

                    channel.BasicPublish(exchange: "", routingKey: "order_created", basicProperties: null, body: body);
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                // TODO: Log the exception
                return StatusCode(500, "An error occurred while testing order creation.");
            }
        }

        [HttpPost("test/submitfeedback")]
        [ProducesResponseType(typeof(Feedback), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public IActionResult TestSubmitFeedback([FromBody] Feedback feedback)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Publish feedback to RabbitMQ
                using (var connection = _rabbitMQFactory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "feedback_submitted", durable: false, exclusive: false, autoDelete: false, arguments: null);

                    var json = JsonSerializer.Serialize(feedback);
                    var body = Encoding.UTF8.GetBytes(json);

                    channel.BasicPublish(exchange: "", routingKey: "feedback_submitted", basicProperties: null, body: body);
                }

                return Ok(feedback);
            }
            catch (Exception ex)
            {
                // TODO: Log the exception
                return StatusCode(500, "An error occurred while testing feedback submission.");
            }
        }

        [HttpGet("test/userdashboard/{userId}")]
        public IActionResult GetUserDashboard(int userId)
        {
            var redis = _redisConnection.GetDatabase();
            var dashboardJson = redis.StringGet($"user_dashboard:{userId}");
            if (dashboardJson.IsNull)
                return NotFound();
            return Ok(JsonSerializer.Deserialize<DashboardData>(dashboardJson));
        }

        [HttpGet("test/inventorystatus")]
        public IActionResult GetInventoryStatus()
        {
            var redis = _redisConnection.GetDatabase();
            var inventoryJson = redis.StringGet("inventory_status");
            if (inventoryJson.IsNull)
                return NotFound();
            return Ok(JsonSerializer.Deserialize<List<Product>>(inventoryJson));
        }

        [HttpGet("test/recentfeedback")]
        public IActionResult GetRecentFeedback()
        {
            var redis = _redisConnection.GetDatabase();
            var feedbackList = redis.ListRange("recent_feedback");
            var feedback = feedbackList.Select(f => JsonSerializer.Deserialize<Feedback>(f));
            return Ok(feedback);
        }
    }
}