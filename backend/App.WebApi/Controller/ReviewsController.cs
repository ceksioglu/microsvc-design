using System.Security.Claims;
using AOP.Aspects;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTO;
using WebAPI.Services.abstracts;

namespace WebAPI.Controller
{
    /// <summary>
    /// Manages product review operations such as creating, updating, and retrieving reviews.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewCommandService _reviewCommandService;
        private readonly IReviewQueryService _reviewQueryService;

        public ReviewsController(IReviewCommandService reviewCommandService, IReviewQueryService reviewQueryService)
        {
            _reviewCommandService = reviewCommandService;
            _reviewQueryService = reviewQueryService;
        }

        /// <summary>
        /// Creates a new review for a product.
        /// </summary>
        /// <param name="reviewDto">The review information to create.</param>
        /// <returns>The created review's information.</returns>
        /// <response code="201">Returns the newly created review.</response>
        /// <response code="400">If the review data is invalid.</response>
        [HttpPost]
        [AuthorizationAspect]
        [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateReview([FromBody] ReviewCreateDto reviewDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            reviewDto.UserId = userId;
            var result = await _reviewCommandService.CreateReviewAsync(reviewDto);
            return CreatedAtAction(nameof(GetReview), new { id = result.Id }, result);
        }

        /// <summary>
        /// Retrieves a specific review by its ID.
        /// </summary>
        /// <param name="id">The ID of the review to retrieve.</param>
        /// <returns>The review's information.</returns>
        /// <response code="200">Returns the requested review.</response>
        /// <response code="404">If the review is not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReview(int id)
        {
            var review = await _reviewQueryService.GetReviewByIdAsync(id);
            return review != null ? Ok(review) : NotFound();
        }

        /// <summary>
        /// Retrieves all reviews for a specific product.
        /// </summary>
        /// <param name="productId">The ID of the product to get reviews for.</param>
        /// <returns>A list of reviews for the specified product.</returns>
        /// <response code="200">Returns the list of reviews for the product.</response>
        [HttpGet("product/{productId}")]
        [ProducesResponseType(typeof(IEnumerable<ReviewResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductReviews(int productId)
        {
            var reviews = await _reviewQueryService.GetReviewsByProductIdAsync(productId);
            return Ok(reviews);
        }

        /// <summary>
        /// Retrieves all reviews by the authenticated user.
        /// </summary>
        /// <returns>A list of reviews by the user.</returns>
        /// <response code="200">Returns the list of reviews by the user.</response>
        [HttpGet("my-reviews")]
        [AuthorizationAspect]
        [ProducesResponseType(typeof(IEnumerable<ReviewResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserReviews()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var reviews = await _reviewQueryService.GetReviewsByUserIdAsync(userId);
            return Ok(reviews);
        }

        /// <summary>
        /// Updates an existing review.
        /// </summary>
        /// <param name="id">The ID of the review to update.</param>
        /// <param name="reviewDto">The updated review information.</param>
        /// <returns>The updated review's information.</returns>
        /// <response code="200">Returns the updated review.</response>
        /// <response code="400">If the update data is invalid.</response>
        /// <response code="404">If the review is not found.</response>
        [HttpPut("{id}")]
        [AuthorizationAspect]
        [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] ReviewUpdateDto reviewDto)
        {
            var updatedReview = await _reviewCommandService.UpdateReviewAsync(id, reviewDto);
            return updatedReview != null ? Ok(updatedReview) : NotFound();
        }

        /// <summary>
        /// Deletes a review.
        /// </summary>
        /// <param name="id">The ID of the review to delete.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the review was successfully deleted.</response>
        /// <response code="404">If the review is not found.</response>
        [HttpDelete("{id}")]
        [AuthorizationAspect]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var result = await _reviewCommandService.DeleteReviewAsync(id);
            return result ? NoContent() : NotFound();
        }

        /// <summary>
        /// Retrieves the average rating for a specific product.
        /// </summary>
        /// <param name="productId">The ID of the product to get the average rating for.</param>
        /// <returns>The average rating for the product.</returns>
        /// <response code="200">Returns the average rating for the product.</response>
        [HttpGet("product/{productId}/average-rating")]
        [ProducesResponseType(typeof(double), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAverageRating(int productId)
        {
            var averageRating = await _reviewQueryService.GetAverageRatingForProductAsync(productId);
            return Ok(averageRating);
        }
    }
}