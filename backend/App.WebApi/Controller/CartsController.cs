using System.Security.Claims;
using AOP.Aspects;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTO;
using WebAPI.Services.abstracts;

namespace WebAPI.Controller
{
    /// <summary>
    /// Manages shopping cart operations such as adding, updating, and removing items.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CartsController : ControllerBase
    {
        private readonly ICartCommandService _cartCommandService;
        private readonly ICartQueryService _cartQueryService;

        public CartsController(ICartCommandService cartCommandService, ICartQueryService cartQueryService)
        {
            _cartCommandService = cartCommandService;
            _cartQueryService = cartQueryService;
        }

        /// <summary>
        /// Retrieves the current user's cart.
        /// </summary>
        /// <returns>The user's cart information.</returns>
        /// <response code="200">Returns the user's cart.</response>
        /// <response code="404">If the cart is not found.</response>
        [HttpGet]
        [AuthorizationAspect]
        [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCart()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var cart = await _cartQueryService.GetCartByUserIdAsync(userId);
            return cart != null ? Ok(cart) : NotFound();
        }

        /// <summary>
        /// Adds an item to the user's cart.
        /// </summary>
        /// <param name="itemDto">The item to add to the cart.</param>
        /// <returns>The updated cart information.</returns>
        /// <response code="200">Returns the updated cart.</response>
        /// <response code="400">If the item data is invalid.</response>
        [HttpPost("items")]
        [AuthorizationAspect]
        [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddToCart([FromBody] CartItemCreateDto itemDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var updatedCart = await _cartCommandService.AddItemToCartAsync(userId, itemDto);
            return Ok(updatedCart);
        }

        /// <summary>
        /// Updates the quantity of an item in the user's cart.
        /// </summary>
        /// <param name="productId">The ID of the product to update.</param>
        /// <param name="itemDto">The updated item information.</param>
        /// <returns>The updated cart information.</returns>
        /// <response code="200">Returns the updated cart.</response>
        /// <response code="400">If the update data is invalid.</response>
        /// <response code="404">If the item is not found in the cart.</response>
        [HttpPut("items/{productId}")]
        [AuthorizationAspect]
        [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCartItem(int productId, [FromBody] CartItemUpdateDto itemDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var updatedCart = await _cartCommandService.UpdateCartItemAsync(userId, productId, itemDto);
            return updatedCart != null ? Ok(updatedCart) : NotFound();
        }

        /// <summary>
        /// Removes an item from the user's cart.
        /// </summary>
        /// <param name="productId">The ID of the product to remove.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the item was successfully removed.</response>
        /// <response code="404">If the item is not found in the cart.</response>
        [HttpDelete("items/{productId}")]
        [AuthorizationAspect]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var result = await _cartCommandService.RemoveItemFromCartAsync(userId, productId);
            return result ? NoContent() : NotFound();
        }

        /// <summary>
        /// Clears all items from the user's cart.
        /// </summary>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the cart was successfully cleared.</response>
        /// <response code="404">If the cart is not found.</response>
        [HttpDelete]
        [AuthorizationAspect]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ClearCart()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var result = await _cartCommandService.ClearCartAsync(userId);
            return result ? NoContent() : NotFound();
        }
    }
}