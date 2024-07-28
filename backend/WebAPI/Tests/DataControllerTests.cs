using WebAPI.Controller;
using WebAPI.Service.abstracts;
using WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RabbitMQ.Client;
using StackExchange.Redis;
using Xunit;
using Order = WebAPI.Models.Order;

namespace WebAPI.Tests
{
    /// <summary>
    /// Contains unit tests for the DataOperationsController class.
    /// These tests cover all major operations of the controller, including
    /// retrieving user dashboard data, inventory status, creating orders,
    /// and submitting feedback.
    /// </summary>
    public class DataOperationsControllerTests
    {
        private readonly Mock<IQueryService> _mockQueryService;
        private readonly Mock<ICommandService> _mockCommandService;
        private readonly Mock<ConnectionFactory> _mockRabbitMQFactory;
        private readonly Mock<IConnectionMultiplexer> _mockRedisConnection;
        private readonly DataOperationsController _controller;

        /// <summary>
        /// Initializes a new instance of the DataOperationsControllerTests class.
        /// Sets up mock services and creates an instance of the controller for testing.
        /// </summary>
        public DataOperationsControllerTests()
        {
            _mockQueryService = new Mock<IQueryService>();
            _mockCommandService = new Mock<ICommandService>();
            _mockRabbitMQFactory = new Mock<ConnectionFactory>();
            _mockRedisConnection = new Mock<IConnectionMultiplexer>();

            _controller = new DataOperationsController(
                _mockQueryService.Object,
                _mockCommandService.Object,
                _mockRabbitMQFactory.Object,
                _mockRedisConnection.Object
            );
        }


        /// <summary>
        /// Tests that GetUserDashboardAsync returns an OK result with the correct DashboardData
        /// when the query service successfully returns dashboard data.
        /// </summary>
        [Fact]
        public async Task GetUserDashboardAsync_ReturnsOkResult_WithDashboardData()
        {
            // Arrange: Set up mock dashboard data
            var dashboardData = new DashboardData
            {
                RecentOrders = new List<OrderSummary>
                {
                    new OrderSummary { OrderId = 1, OrderDate = DateTime.Now, TotalAmount = 100.00m, Status = "Completed" }
                },
                TotalOrders = 1,
                TotalSpent = 100.00m
            };

            _mockQueryService.Setup(service => service.GetUserDashboardAsync())
                .ReturnsAsync(dashboardData);

            // Act: Call the controller method
            var result = await _controller.GetUserDashboardAsync();

            // Assert: Verify the result type and content
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<DashboardData>(okResult.Value);
            Assert.Equal(dashboardData.TotalOrders, returnValue.TotalOrders);
            Assert.Equal(dashboardData.TotalSpent, returnValue.TotalSpent);
        }

        /// <summary>
        /// Tests that GetUserDashboardAsync returns an Internal Server Error result
        /// when an exception occurs in the query service.
        /// </summary>
        [Fact]
        public async Task GetUserDashboardAsync_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange: Set up mock service to throw an exception
            _mockQueryService.Setup(service => service.GetUserDashboardAsync())
                .ThrowsAsync(new Exception("Test exception"));

            // Act: Call the controller method
            var result = await _controller.GetUserDashboardAsync();

            // Assert: Verify the result type and content
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while fetching the user dashboard.", statusCodeResult.Value);
        }

        /// <summary>
        /// Tests that GetInventoryStatusAsync returns an OK result with the correct List of Products
        /// when the query service successfully returns inventory data.
        /// </summary>
        [Fact]
        public async Task GetInventoryStatusAsync_ReturnsOkResult_WithListOfProducts()
        {
            // Arrange: Set up mock inventory data
            var inventoryData = new List<Product>
            {
                new Product { Id = 1, Name = "Test Product", StockQuantity = 10, Price = 19.99m }
            };

            _mockQueryService.Setup(service => service.GetInventoryStatusAsync())
                .ReturnsAsync(inventoryData);

            // Act: Call the controller method
            var result = await _controller.GetInventoryStatusAsync();

            // Assert: Verify the result type and content
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<Product>>(okResult.Value);
            Assert.Single(returnValue);
            Assert.Equal(inventoryData[0].Id, returnValue[0].Id);
            Assert.Equal(inventoryData[0].Name, returnValue[0].Name);
        }

        /// <summary>
        /// Tests that GetInventoryStatusAsync returns an Internal Server Error result
        /// when an exception occurs in the query service.
        /// </summary>
        [Fact]
        public async Task GetInventoryStatusAsync_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange: Set up mock service to throw an exception
            _mockQueryService.Setup(service => service.GetInventoryStatusAsync())
                .ThrowsAsync(new Exception("Test exception"));

            // Act: Call the controller method
            var result = await _controller.GetInventoryStatusAsync();

            // Assert: Verify the result type and content
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while fetching the inventory status.", statusCodeResult.Value);
        }

        /// <summary>
        /// Tests that CreateOrderAsync returns an OK result with the created Order
        /// when the command service successfully processes the order creation request.
        /// </summary>
        [Fact]
        public async Task CreateOrderAsync_ReturnsOkResult_WithCreatedOrder()
        {
            // Arrange: Set up mock order and service response
            var order = new Order
            {
                Id = 1,
                OrderDate = DateTime.Now,
                Status = "Created",
                TotalAmount = 39.98m,
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 2, UnitPrice = 19.99m }
                },
                ShippingAddress = new ShippingAddress
                {
                    FullName = "Test User",
                    AddressLine1 = "123 Test St",
                    City = "Test City",
                    State = "TS",
                    PostalCode = "12345",
                    Country = "Test Country"
                }
            };

            _mockCommandService.Setup(service => service.CreateOrderAsync(It.IsAny<Order>()))
                .ReturnsAsync(order);

            // Act: Call the controller method
            var result = await _controller.CreateOrderAsync(order);

            // Assert: Verify the result type and content
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<Order>(okResult.Value);
            Assert.Equal(order.Id, returnValue.Id);
            Assert.Equal(order.TotalAmount, returnValue.TotalAmount);
        }

        /// <summary>
        /// Tests that CreateOrderAsync returns a Bad Request result
        /// when the model state is invalid.
        /// </summary>
        [Fact]
        public async Task CreateOrderAsync_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange: Add a model error to simulate invalid model state
            _controller.ModelState.AddModelError("TotalAmount", "TotalAmount is required");

            // Act: Call the controller method with an empty order
            var result = await _controller.CreateOrderAsync(new Order());

            // Assert: Verify the result type
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that SubmitFeedbackAsync returns an OK result with the submitted Feedback
        /// when the command service successfully processes the feedback submission request.
        /// </summary>
        [Fact]
        public async Task SubmitFeedbackAsync_ReturnsOkResult_WithSubmittedFeedback()
        {
            // Arrange: Set up mock feedback and service response
            var feedback = new Feedback
            {
                Id = 1,
                UserId = 1,
                Category = "Product",
                Comment = "Great product!",
                Rating = 5,
                SubmissionDate = DateTime.Now
            };

            _mockCommandService.Setup(service => service.SubmitFeedbackAsync(It.IsAny<Feedback>()))
                .ReturnsAsync(feedback);

            // Act: Call the controller method
            var result = await _controller.SubmitFeedbackAsync(feedback);

            // Assert: Verify the result type and content
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<Feedback>(okResult.Value);
            Assert.Equal(feedback.Id, returnValue.Id);
            Assert.Equal(feedback.Comment, returnValue.Comment);
        }

        /// <summary>
        /// Tests that SubmitFeedbackAsync returns a Bad Request result
        /// when the model state is invalid.
        /// </summary>
        [Fact]
        public async Task SubmitFeedbackAsync_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange: Add a model error to simulate invalid model state
            _controller.ModelState.AddModelError("Rating", "Rating is required");

            // Act: Call the controller method with an empty feedback
            var result = await _controller.SubmitFeedbackAsync(new Feedback());

            // Assert: Verify the result type
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}