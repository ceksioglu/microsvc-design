using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.Service.abstracts;

namespace WebAPI.Controller
{
    /// <summary>
    /// Controller responsible for handling data operations in the e-commerce microservice architecture.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DataOperationsController : ControllerBase
    {
        private readonly IQueryService _queryService;
        private readonly ICommandService _commandService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataOperationsController"/> class.
        /// </summary>
        /// <param name="queryService">The service responsible for handling read operations.</param>
        /// <param name="commandService">The service responsible for handling write operations.</param>
        /// <exception cref="ArgumentNullException">Thrown if either service is null.</exception>
        public DataOperationsController(IQueryService queryService, ICommandService commandService)
        {
            _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        }

        /// <summary>
        /// Retrieves the user dashboard data.
        /// </summary>
        /// <returns>A <see cref="UserDashboardResponse"/> containing the user's dashboard data.</returns>
        /// <response code="200">Returns the user dashboard data.</response>
        /// <response code="500">If there was an internal server error.</response>
        [HttpGet("userdashboard")]
        [ProducesResponseType(typeof(UserDashboardResponse), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetUserDashboardAsync()
        {
            try
            {
                var dashboardData = await _queryService.GetUserDashboardAsync();
                return Ok(new UserDashboardResponse { DashboardData = dashboardData });
            }
            catch (Exception ex)
            {
                // TODO: Log the exception
                return StatusCode(500, "An error occurred while fetching the user dashboard.");
            }
        }

        /// <summary>
        /// Retrieves the current inventory status.
        /// </summary>
        /// <returns>An <see cref="InventoryStatusResponse"/> containing the current inventory data.</returns>
        /// <response code="200">Returns the inventory status data.</response>
        /// <response code="500">If there was an internal server error.</response>
        [HttpGet("inventorystatus")]
        [ProducesResponseType(typeof(InventoryStatusResponse), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetInventoryStatusAsync()
        {
            try
            {
                var inventoryData = await _queryService.GetInventoryStatusAsync();
                return Ok(new InventoryStatusResponse { InventoryData = inventoryData });
            }
            catch (Exception ex)
            {
                // TODO: Log the exception
                return StatusCode(500, "An error occurred while fetching the inventory status.");
            }
        }

        /// <summary>
        /// Creates a new order in the system.
        /// </summary>
        /// <param name="request">The <see cref="CreateOrderRequest"/> containing the order details.</param>
        /// <returns>A <see cref="CreateOrderResponse"/> indicating the success or failure of the operation.</returns>
        /// <response code="200">If the order was created successfully.</response>
        /// <response code="400">If the request model is invalid.</response>
        /// <response code="500">If there was an internal server error.</response>
        [HttpPost("createorder")]
        [ProducesResponseType(typeof(CreateOrderResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateOrderAsync([FromBody] CreateOrderRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _commandService.CreateOrderAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // TODO: Log the exception
                return StatusCode(500, "An error occurred while creating the order.");
            }
        }

        /// <summary>
        /// Submits customer feedback.
        /// </summary>
        /// <param name="request">The <see cref="SubmitFeedbackRequest"/> containing the feedback details.</param>
        /// <returns>A <see cref="SubmitFeedbackResponse"/> indicating the success or failure of the operation.</returns>
        /// <response code="200">If the feedback was submitted successfully.</response>
        /// <response code="400">If the request model is invalid.</response>
        /// <response code="500">If there was an internal server error.</response>
        [HttpPost("submitfeedback")]
        [ProducesResponseType(typeof(SubmitFeedbackResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SubmitFeedbackAsync([FromBody] SubmitFeedbackRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _commandService.SubmitFeedbackAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // TODO: Log the exception
                return StatusCode(500, "An error occurred while submitting the feedback.");
            }
        }
    }
}