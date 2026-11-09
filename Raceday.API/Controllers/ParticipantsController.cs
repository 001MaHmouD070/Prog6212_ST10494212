using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Raceday.API.DTOs;
using Raceday.API.Services;
using RacedayAPI.Data;

namespace RacedayAPI.Controllers
{
    [ApiController]
    [Route("api/participants")]
    public class ParticipantsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ParticipantsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: /api/participants/profile — PARTICIPANT ONLY
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            if (userId == null)
                return Unauthorized(new { message = "You must be logged in." });

            if (!SessionHelper.IsInRole(HttpContext, "Participant"))
                return StatusCode(403, new { message = "Only Participants can access this endpoint." });

            var participant = await _context.Participants
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserID == userId.Value);

            if (participant == null)
                return NotFound(new { message = "Participant profile not found." });

            return Ok(new
            {
                participant.ParticipantID,
                participant.DateOfBirth,
                participant.Gender,
                participant.EmergencyContact,
                participant.MedicalConditions,
                User = participant.User == null ? null : new
                {
                    participant.User.UserID,
                    participant.User.Email,
                    participant.User.FullName,
                    participant.User.Role
                }
            });
        }

        // PUT: /api/participants/profile — PARTICIPANT ONLY
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateParticipantDto dto)
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            if (userId == null)
                return Unauthorized(new { message = "You must be logged in." });

            if (!SessionHelper.IsInRole(HttpContext, "Participant"))
                return StatusCode(403, new { message = "Only Participants can update this profile." });

            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.UserID == userId.Value);

            if (participant == null)
                return NotFound(new { message = "Participant profile not found." });

            participant.DateOfBirth = dto.DateOfBirth;
            participant.Gender = dto.Gender;
            participant.EmergencyContact = dto.EmergencyContact;
            participant.MedicalConditions = dto.MedicalConditions;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                participant.ParticipantID,
                participant.DateOfBirth,
                participant.Gender,
                participant.EmergencyContact,
                participant.MedicalConditions
            });
        }
    }
}