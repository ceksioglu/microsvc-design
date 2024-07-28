using WebAPI.Controller;
using WebAPI.Service.abstracts;
using WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using System.Threading.Tasks;

namespace WebAPI.Tests
{
    public class DataOperationsControllerTests
    {
        private readonly Mock<IQueryService> _mockQueryService;
        private readonly Mock<ICommandService> _mockCommandService;
        private readonly DataOperationsController _controller;

        public DataOperationsControllerTests()
        {
            _mockQueryService = new Mock<IQueryService>();
            _mockCommandService = new Mock<ICommandService>();
            _controller = new DataOperationsController(_mockQueryService.Object, _mockCommandService.Object);
        }

        [Fact]
        public async Task GetUserDashboardAsync_ReturnsOkResult_WithUserDashboardResponse()
        {
            // Arrange
            var dashboardData = new DashboardData
            {
                UserId = "123",
                Username = "testuser",
                RecentOrders = new List<OrderSummary>
                {
                    new OrderSummary { OrderId = "ORDER1", TotalAmount = 100.00m }
                },
                TotalOrders = 1,
                TotalSpent = 100.00m
            };

            _mockQueryService.Setup(service => service.GetUserDashboardAsync())
                .ReturnsAsync(dashboardData);

            // Act
            var result = await _controller.GetUserDashboardAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<UserDashboardResponse>(okResult.Value);
            Assert.Equal(dashboardData, returnValue.DashboardData);
        }

        [Fact]
        public async Task GetUserDashboardAsync_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            _mockQueryService.Setup(service => service.GetUserDashboardAsync())
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetUserDashboardAsync();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while fetching the user dashboard.", statusCodeResult.Value);
        }

        [Fact]
        public async Task GetInventoryStatusAsync_ReturnsOkResult_WithInventoryStatusResponse()
        {
            // Arrange
            var inventoryData = new InventoryData
            {
                Products = new List<ProductInventory>
                {
                    new ProductInventory { ProductId = "PROD1", StockQuantity = 10 }
                },
                TotalProducts = 1,
                LowStockCount = 0
            };

            _mockQueryService.Setup(service => service.GetInventoryStatusAsync())
                .ReturnsAsync(inventoryData);

            // Act
            var result = await _controller.GetInventoryStatusAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<InventoryStatusResponse>(okResult.Value);
            Assert.Equal(inventoryData, returnValue.InventoryData);
        }

        [Fact]
        public async Task GetInventoryStatusAsync_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            _mockQueryService.Setup(service => service.GetInventoryStatusAsync())
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetInventoryStatusAsync();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while fetching the inventory status.", statusCodeResult.Value);
        }

        [Fact]
        public async Task CreateOrderAsync_ReturnsOkResult_WithCreateOrderResponse()
        {
            // Arrange
            var orderRequest = new CreateOrderRequest
            {
                OrderId = "ORDER1",
                CustomerId = "CUST1",
                TotalAmount = 100.00m
            };

            var orderResponse = new CreateOrderResponse
            {
                Success = true,
                Message = "Order created successfully",
                OrderId = "ORDER1"
            };

            _mockCommandService.Setup(service => service.CreateOrderAsync(It.IsAny<CreateOrderRequest>()))
                .ReturnsAsync(orderResponse);

            // Act
            var result = await _controller.CreateOrderAsync(orderRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<CreateOrderResponse>(okResult.Value);
            Assert.Equal(orderResponse, returnValue);
        }

        [Fact]
        public async Task CreateOrderAsync_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("OrderId", "OrderId is required");

            // Act
            var result = await _controller.CreateOrderAsync(new CreateOrderRequest());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task SubmitFeedbackAsync_ReturnsOkResult_WithSubmitFeedbackResponse()
        {
            // Arrange
            var feedbackRequest = new SubmitFeedbackRequest
            {
                FeedbackId = "FB1",
                CustomerId = "CUST1",
                Rating = 5
            };

            var feedbackResponse = new SubmitFeedbackResponse
            {
                Success = true,
                Message = "Feedback submitted successfully",
                FeedbackId = "FB1"
            };

            _mockCommandService.Setup(service => service.SubmitFeedbackAsync(It.IsAny<SubmitFeedbackRequest>()))
                .ReturnsAsync(feedbackResponse);

            // Act
            var result = await _controller.SubmitFeedbackAsync(feedbackRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<SubmitFeedbackResponse>(okResult.Value);
            Assert.Equal(feedbackResponse, returnValue);
        }

        [Fact]
        public async Task SubmitFeedbackAsync_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Rating", "Rating is required");

            // Act
            var result = await _controller.SubmitFeedbackAsync(new SubmitFeedbackRequest());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}