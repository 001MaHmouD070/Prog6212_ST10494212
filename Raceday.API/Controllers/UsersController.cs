using Microsoft.AspNetCore.Mvc;
using Raceday.API.DTOs;
using Raceday.API.Services;
using RacedayAPI.Data;

namespace RacedayAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public UsersController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: /api/users/profile — Any logged-in user
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            if (userId == null)
                return Unauthorized(new { message = "You must be logged in." });

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(new
            {
                user.UserID,
                user.Email,
                user.FullName,
                user.Role,
                user.CreatedAt
            });
        }

        // PUT: /api/users/profile — Any logged-in user
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto dto)
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            if (userId == null)
                return Unauthorized(new { message = "You must be logged in." });

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
                return NotFound(new { message = "User not found." });

            user.FullName = dto.FullName;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                user.UserID,
                user.Email,
                user.FullName,
                user.Role,
                user.CreatedAt
            });
        }
    }
}