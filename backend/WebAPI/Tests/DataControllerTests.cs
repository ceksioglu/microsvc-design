using WebAPI.Controller;
using WebAPI.Service.abstracts;
using WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        private readonly DataOperationsController _controller;

        /// <summary>
        /// Initializes a new instance of the DataOperationsControllerTests class.
        /// Sets up mock services and creates an instance of the controller for testing.
        /// </summary>
        public DataOperationsControllerTests()
        {
            _mockQueryService = new Mock<IQueryService>();
            _mockCommandService = new Mock<ICommandService>();
            _controller = new DataOperationsController(_mockQueryService.Object, _mockCommandService.Object);
        }

        /// <summary>
        /// Tests that GetUserDashboardAsync returns an OK result with the correct UserDashboardResponse
        /// when the query service successfully returns dashboard data.
        /// </summary>
        [Fact]
        public async Task GetUserDashboardAsync_ReturnsOkResult_WithUserDashboardResponse()
        {
            // Arrange: Set up mock dashboard data
            var dashboardData = new DashboardData
            {
                UserId = "123",
                Username = "testuser",
                RecentOrders = new List<OrderSummary>
                {
                    new OrderSummary { OrderId = "ORDER1", OrderDate = DateTime.Now, TotalAmount = 100.00m, Status = "Completed" }
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
            var returnValue = Assert.IsType<UserDashboardResponse>(okResult.Value);
            Assert.Equal(dashboardData.UserId, returnValue.DashboardData.UserId);
            Assert.Equal(dashboardData.Username, returnValue.DashboardData.Username);
            Assert.Equal(dashboardData.TotalOrders, returnValue.DashboardData.TotalOrders);
            Assert.Equal(dashboardData.TotalSpent, returnValue.DashboardData.TotalSpent);
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
        /// Tests that GetInventoryStatusAsync returns an OK result with the correct InventoryStatusResponse
        /// when the query service successfully returns inventory data.
        /// </summary>
        [Fact]
        public async Task GetInventoryStatusAsync_ReturnsOkResult_WithInventoryStatusResponse()
        {
            // Arrange: Set up mock inventory data
            var inventoryData = new InventoryData
            {
                Products = new List<ProductInventory>
                {
                    new ProductInventory { ProductId = "PROD1", ProductName = "Test Product", StockQuantity = 10, Price = 19.99m }
                },
                TotalProducts = 1,
                LowStockCount = 0
            };

            _mockQueryService.Setup(service => service.GetInventoryStatusAsync())
                .ReturnsAsync(inventoryData);

            // Act: Call the controller method
            var result = await _controller.GetInventoryStatusAsync();

            // Assert: Verify the result type and content
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<InventoryStatusResponse>(okResult.Value);
            Assert.Equal(inventoryData.TotalProducts, returnValue.InventoryData.TotalProducts);
            Assert.Equal(inventoryData.LowStockCount, returnValue.InventoryData.LowStockCount);
            Assert.Equal(inventoryData.Products[0].ProductId, returnValue.InventoryData.Products[0].ProductId);
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
        /// Tests that CreateOrderAsync returns an OK result with a boolean value
        /// when the command service successfully processes the order creation request.
        /// </summary>
        [Fact]
        public async Task CreateOrderAsync_ReturnsOkResult_WithBooleanValue()
        {
            // Arrange: Set up mock order request and service response
            var orderRequest = new CreateOrderRequest
            {
                OrderId = "ORDER1",
                CustomerId = "CUST1",
                OrderDate = DateTime.Now,
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = "PROD1", ProductName = "Test Product", Quantity = 2, UnitPrice = 19.99m }
                },
                ShippingAddress = new ShippingAddress
                {
                    Street = "123 Test St",
                    City = "Test City",
                    State = "TS",
                    ZipCode = "12345",
                    Country = "Test Country"
                },
                PaymentMethod = "Credit Card",
                TotalAmount = 39.98m
            };

            _mockCommandService.Setup(service => service.CreateOrderAsync(It.IsAny<CreateOrderRequest>()))
                .ReturnsAsync(true);

            // Act: Call the controller method
            var result = await _controller.CreateOrderAsync(orderRequest);

            // Assert: Verify the result type and content
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<bool>(okResult.Value);
            Assert.True(returnValue);
        }

        /// <summary>
        /// Tests that CreateOrderAsync returns a Bad Request result
        /// when the model state is invalid.
        /// </summary>
        [Fact]
        public async Task CreateOrderAsync_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange: Add a model error to simulate invalid model state
            _controller.ModelState.AddModelError("OrderId", "OrderId is required");

            // Act: Call the controller method with an empty request
            var result = await _controller.CreateOrderAsync(new CreateOrderRequest());

            // Assert: Verify the result type
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that SubmitFeedbackAsync returns an OK result with a boolean value
        /// when the command service successfully processes the feedback submission request.
        /// </summary>
        [Fact]
        public async Task SubmitFeedbackAsync_ReturnsOkResult_WithBooleanValue()
        {
            // Arrange: Set up mock feedback request and service response
            var feedbackRequest = new SubmitFeedbackRequest
            {
                FeedbackId = "FB1",
                CustomerId = "CUST1",
                OrderReference = "ORDER1",
                SubmissionDate = DateTime.Now,
                OverallRating = 5,
                Categories = new FeedbackCategories
                {
                    ProductQuality = 5,
                    Delivery = 4,
                    CustomerService = 5
                },
                Comments = "Great service!",
                WouldRecommend = true,
                Tags = new List<string> { "fast", "quality" }
            };

            _mockCommandService.Setup(service => service.SubmitFeedbackAsync(It.IsAny<SubmitFeedbackRequest>()))
                .ReturnsAsync(true);

            // Act: Call the controller method
            var result = await _controller.SubmitFeedbackAsync(feedbackRequest);

            // Assert: Verify the result type and content
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<bool>(okResult.Value);
            Assert.True(returnValue);
        }

        /// <summary>
        /// Tests that SubmitFeedbackAsync returns a Bad Request result
        /// when the model state is invalid.
        /// </summary>
        [Fact]
        public async Task SubmitFeedbackAsync_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange: Add a model error to simulate invalid model state
            _controller.ModelState.AddModelError("OverallRating", "OverallRating is required");

            // Act: Call the controller method with an empty request
            var result = await _controller.SubmitFeedbackAsync(new SubmitFeedbackRequest());

            // Assert: Verify the result type
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}