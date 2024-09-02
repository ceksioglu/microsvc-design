using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AOP.Aspects;
using DataAccess.DTO;
using Microsoft.AspNetCore.Mvc;
using Services.Services.abstracts;

namespace WebAPI.Controller
{
    /// <summary>
    /// Manages user-related operations such as registration, authentication, and profile management.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IUserCommandService _userCommandService;
        private readonly IUserQueryService _userQueryService;

        public UsersController(IUserCommandService userCommandService, IUserQueryService userQueryService)
        {
            _userCommandService = userCommandService;
            _userQueryService = userQueryService;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="userDto">The user information for registration.</param>
        /// <returns>The created user's information.</returns>
        /// <response code="201">Returns the newly created user.</response>
        /// <response code="400">If the user data is invalid.</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] UserCreateDto userDto)
        {
            var result = await _userCommandService.RegisterUserAsync(userDto);
            return CreatedAtAction(nameof(GetUser), new { id = result.Id }, result);
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <param name="loginDto">The login credentials.</param>
        /// <returns>A JWT token for the authenticated user.</returns>
        /// <response code="200">Returns the JWT token.</response>
        /// <response code="401">If the credentials are invalid.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var token = await _userCommandService.AuthenticateUserAsync(loginDto.Email, loginDto.Password);
            return Ok(new LoginResponseDto { Token = token });
        }

        /// <summary>
        /// Retrieves a specific user by their ID.
        /// </summary>
        /// <param name="id">The ID of the user to retrieve.</param>
        /// <returns>The user's information.</returns>
        /// <response code="200">Returns the requested user.</response>
        /// <response code="404">If the user is not found.</response>
        [HttpGet("{id}")]
        [AuthorizationAspect]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userQueryService.GetUserByIdAsync(id);
            return user != null ? Ok(user) : NotFound();
        }

        /// <summary>
        /// Retrieves all users. Accessible only by administrators.
        /// </summary>
        /// <returns>A list of all users.</returns>
        /// <response code="200">Returns the list of all users.</response>
        /// <response code="403">If the user is not an administrator.</response>
        [HttpGet]
        [AuthorizationAspect("Admin")]
        [ProducesResponseType(typeof(IEnumerable<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userQueryService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Updates a user's information.
        /// </summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="userDto">The updated user information.</param>
        /// <returns>The updated user's information.</returns>
        /// <response code="200">200 Returns the updated user.</response>
        /// <response code="400">400 If the update data is invalid.</response>
        /// <response code="404">404 If the user is not found.</response>
        [HttpPut("{id}")]
        [AuthorizationAspect]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto userDto)
        {
            var updatedUser = await _userCommandService.UpdateUserAsync(id, userDto);
            return updatedUser != null ? Ok(updatedUser) : NotFound();
        }

        /// <summary>
        /// Deletes a user. Accessible only by administrators.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the user was successfully deleted.</response>
        /// <response code="404">If the user is not found.</response>
        /// <response code="403">If the user is not an administrator.</response>
        [HttpDelete("{id}")]
        [AuthorizationAspect("Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userCommandService.DeleteUserAsync(id);
            return result ? NoContent() : NotFound();
        }

        /// <summary>
        /// Changes the password for the authenticated user.
        /// </summary>
        /// <param name="changePasswordDto">The current and new password.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the password was successfully changed.</response>
        /// <response code="400">If the password change failed.</response>
        [HttpPost("change-password")]
        [AuthorizationAspect]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var result = await _userCommandService.ChangePasswordAsync(userId, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
            return result ? NoContent() : BadRequest("Failed to change password");
        }
    }

    /// <summary>
    /// Represents the login credentials for user authentication.
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// The user's email address.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// The user's password.
        /// </summary>
        [Required]
        public string Password { get; set; }
    }

    /// <summary>
    /// Represents the response after successful login.
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>
        /// The JWT token for the authenticated user.
        /// </summary>
        public string Token { get; set; }
    }

    /// <summary>
    /// Represents the data required for changing a user's password.
    /// </summary>
    public class ChangePasswordDto
    {
        /// <summary>
        /// The user's current password.
        /// </summary>
        [Required]
        public string CurrentPassword { get; set; }

        /// <summary>
        /// The new password to set.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; }
    }
}