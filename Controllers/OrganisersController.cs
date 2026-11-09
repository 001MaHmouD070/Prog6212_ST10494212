using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Raceday.API.DTOs;
using Raceday.API.Services;
using RacedayAPI.Data;

namespace RacedayAPI.Controllers
{
    [ApiController]
    [Route("api/organisers")]
    public class OrganisersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public OrganisersController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: /api/organisers/profile — ORGANISER ONLY
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            if (userId == null)
                return Unauthorized(new { message = "You must be logged in." });

            if (!SessionHelper.IsInRole(HttpContext, "Organiser"))
                return StatusCode(403, new { message = "Only Organisers can access this endpoint." });

            var organiser = await _context.Organisers
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.UserID == userId.Value);

            if (organiser == null)
                return NotFound(new { message = "Organiser profile not found." });

            return Ok(new
            {
                organiser.OrganiserID,
                organiser.CompanyName,
                organiser.PhoneNumber,
                organiser.IsVerified,
                User = organiser.User == null ? null : new
                {
                    organiser.User.UserID,
                    organiser.User.Email,
                    organiser.User.FullName,
                    organiser.User.Role
                }
            });
        }

        // PUT: /api/organisers/profile — ORGANISER ONLY
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateOrganiserDto dto)
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            if (userId == null)
                return Unauthorized(new { message = "You must be logged in." });

            if (!SessionHelper.IsInRole(HttpContext, "Organiser"))
                return StatusCode(403, new { message = "Only Organisers can update this profile." });

            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserID == userId.Value);

            if (organiser == null)
                return NotFound(new { message = "Organiser profile not found." });

            organiser.CompanyName = dto.CompanyName;
            organiser.PhoneNumber = dto.PhoneNumber;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                organiser.OrganiserID,
                organiser.CompanyName,
                organiser.PhoneNumber,
                organiser.IsVerified
            });
        }
    }
}