using Microsoft.AspNetCore.Mvc;
using Raceday.API.DTOs;
using RacedayAPI.Services;

namespace RacedayAPI.Controllers
{
    /// <summary>
    /// Handles user registration, login, logout, and session management.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user as either an Organiser or a Participant.
        /// </summary>
        /// <param name="registerDto">Registration details including email, password, full name, and role.</param>
        /// <returns>The newly created user (without password).</returns>
        /// <response code="201">User registered successfully.</response>
        /// <response code="400">Invalid input or email already exists.</response>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(registerDto);

            if (result == null)
                return BadRequest(new { message = "Email already exists or invalid role." });

            return CreatedAtAction(nameof(GetCurrentUser), new { id = result.UserID }, result);
        }

        /// <summary>
        /// Authenticates a user and creates a server-side session. 
        /// The session cookie is sent back as Set-Cookie and must be included in subsequent requests.
        /// </summary>
        /// <param name="loginDto">User's email and password.</param>
        /// <returns>The authenticated user's details.</returns>
        /// <response code="200">Login successful. Session cookie is set.</response>
        /// <response code="401">Invalid credentials.</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(loginDto);

            if (result == null)
                return Unauthorized(new { message = "Invalid email or password." });

            HttpContext.Session.SetInt32("UserID", result.UserID);
            HttpContext.Session.SetString("Role", result.Role);
            HttpContext.Session.SetString("Email", result.Email);
            HttpContext.Session.SetString("FullName", result.FullName);

            return Ok(new { message = "Login successful.", user = result });
        }

        /// <summary>
        /// Logs out the current user by clearing their session.
        /// </summary>
        /// <returns>A confirmation message.</returns>
        /// <response code="200">Logout successful.</response>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logout successful." });
        }

        /// <summary>
        /// Returns the currently logged-in user's profile from the session.
        /// </summary>
        /// <returns>The current user's details.</returns>
        /// <response code="200">User details returned.</response>
        /// <response code="401">Not logged in.</response>
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = HttpContext.Session.GetInt32("UserID");

            if (userId == null)
                return Unauthorized(new { message = "Not logged in." });

            var user = await _authService.GetUserByIdAsync(userId.Value);

            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(user);
        }
    }
}