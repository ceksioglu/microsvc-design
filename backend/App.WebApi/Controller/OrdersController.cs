using System.Security.Claims;
using AOP.Aspects;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTO;
using WebAPI.Services.abstracts;

namespace WebAPI.Controller
{
    /// <summary>
    /// Manages order-related operations such as creation, retrieval, and status updates.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderCommandService _orderCommandService;
        private readonly IOrderQueryService _orderQueryService;

        public OrdersController(IOrderCommandService orderCommandService, IOrderQueryService orderQueryService)
        {
            _orderCommandService = orderCommandService;
            _orderQueryService = orderQueryService;
        }

        /// <summary>
        /// Creates a new order.
        /// </summary>
        /// <param name="orderDto">The order information to create.</param>
        /// <returns>The created order's information.</returns>
        /// <response code="201">Returns the newly created order.</response>
        /// <response code="400">If the order data is invalid.</response>
        [HttpPost]
        [AuthorizationAspect]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDto orderDto)
        {
            var result = await _orderCommandService.CreateOrderAsync(orderDto);
            return CreatedAtAction(nameof(GetOrder), new { id = result.Id }, result);
        }

        /// <summary>
        /// Retrieves a specific order by its ID.
        /// </summary>
        /// <param name="id">The ID of the order to retrieve.</param>
        /// <returns>The order's information.</returns>
        /// <response code="200">Returns the requested order.</response>
        /// <response code="404">If the order is not found.</response>
        [HttpGet("{id}")]
        [AuthorizationAspect]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _orderQueryService.GetOrderByIdAsync(id);
            return order != null ? Ok(order) : NotFound();
        }

        /// <summary>
        /// Retrieves all orders. Accessible only by administrators.
        /// </summary>
        /// <returns>A list of all orders.</returns>
        /// <response code="200">Returns the list of all orders.</response>
        /// <response code="403">If the user is not an administrator.</response>
        [HttpGet]
        [AuthorizationAspect("Admin")]
        [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderQueryService.GetAllOrdersAsync();
            return Ok(orders);
        }

        /// <summary>
        /// Retrieves orders for the authenticated user.
        /// </summary>
        /// <returns>A list of orders for the authenticated user.</returns>
        /// <response code="200">Returns the list of orders for the user.</response>
        [HttpGet("my-orders")]
        [AuthorizationAspect]
        [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserOrders()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var orders = await _orderQueryService.GetOrdersByUserIdAsync(userId);
            return Ok(orders);
        }

        /// <summary>
        /// Updates the status of an order.
        /// </summary>
        /// <param name="id">The ID of the order to update.</param>
        /// <param name="status">The new status of the order.</param>
        /// <returns>The updated order's information.</returns>
        /// <response code="200">Returns the updated order.</response>
        /// <response code="400">If the update data is invalid.</response>
        /// <response code="404">If the order is not found.</response>
        [HttpPatch("{id}/status")]
        [AuthorizationAspect("Admin")]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            var updatedOrder = await _orderCommandService.UpdateOrderStatusAsync(id, status);
            return updatedOrder != null ? Ok(updatedOrder) : NotFound();
        }

        /// <summary>
        /// Cancels an order.
        /// </summary>
        /// <param name="id">The ID of the order to cancel.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the order was successfully cancelled.</response>
        /// <response code="404">If the order is not found.</response>
        [HttpDelete("{id}")]
        [AuthorizationAspect]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var result = await _orderCommandService.CancelOrderAsync(id);
            return result ? NoContent() : NotFound();
        }
    }
}