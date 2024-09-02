using System.Security.Claims;
using AOP.Aspects;
using DataAccess.DTO;
using Microsoft.AspNetCore.Mvc;
using Services.Services.abstracts;

namespace WebAPI.Controller
{
    /// <summary>
    /// Controller for managing customer support tickets.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CustomerSupportController : ControllerBase
    {
        private readonly ICustomerSupportService _customerSupportService;

        /// <summary>
        /// Initializes a new instance of the CustomerSupportController.
        /// </summary>
        /// <param name="customerSupportService">The customer support service.</param>
        public CustomerSupportController(ICustomerSupportService customerSupportService)
        {
            _customerSupportService = customerSupportService;
        }

        /// <summary>
        /// Creates a new customer support ticket.
        /// </summary>
        /// <param name="ticketDto">The ticket information.</param>
        /// <returns>The created ticket.</returns>
        /// <response code="201">Returns the newly created ticket</response>
        /// <response code="400">If the ticket data is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpPost]
        [AuthorizationAspect]
        [ProducesResponseType(typeof(CustomerSupportTicketResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<CustomerSupportTicketResponseDto>> CreateTicket([FromBody] CustomerSupportTicketCreateDto ticketDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
                return Unauthorized("User ID not found in token.");

            ticketDto.UserId = userId;

            var result = await _customerSupportService.CreateTicketAsync(ticketDto);
            return CreatedAtAction(nameof(GetTicket), new { id = result.Id }, result);
        }

        /// <summary>
        /// Retrieves a specific customer support ticket.
        /// </summary>
        /// <param name="id">The ID of the ticket to retrieve.</param>
        /// <returns>The requested ticket.</returns>
        /// <response code="200">Returns the requested ticket</response>
        /// <response code="404">If the ticket is not found</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpGet("{id}")]
        [AuthorizationAspect]
        [ProducesResponseType(typeof(CustomerSupportTicketResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<CustomerSupportTicketResponseDto>> GetTicket(int id)
        {
            var ticket = await _customerSupportService.GetTicketByIdAsync(id);
            if (ticket == null)
                return NotFound();

            return Ok(ticket);
        }

        /// <summary>
        /// Resolves a customer support ticket.
        /// </summary>
        /// <param name="id">The ID of the ticket to resolve.</param>
        /// <param name="resolution">The resolution details.</param>
        /// <returns>The resolved ticket.</returns>
        /// <response code="200">Returns the resolved ticket</response>
        /// <response code="404">If the ticket is not found</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not an admin</response>
        [HttpPut("{id}/resolve")]
        [AuthorizationAspect("Admin")]
        [ProducesResponseType(typeof(CustomerSupportTicketResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CustomerSupportTicketResponseDto>> ResolveTicket(int id, [FromBody] string resolution)
        {
            var result = await _customerSupportService.ResolveTicketAsync(id, resolution);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        /// <summary>
        /// Retrieves all customer support tickets.
        /// </summary>
        /// <returns>A list of all tickets.</returns>
        /// <response code="200">Returns the list of all tickets</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not an admin</response>
        [HttpGet]
        [AuthorizationAspect("Admin")]
        [ProducesResponseType(typeof(IEnumerable<CustomerSupportTicketResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<CustomerSupportTicketResponseDto>>> GetAllTickets()
        {
            var tickets = await _customerSupportService.GetAllTicketsAsync();
            return Ok(tickets);
        }
    }
}