using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Raceday.API.DTOs;
using Raceday.API.Services;
using RacedayAPI.Data;
using RacedayAPI.Models;

namespace RacedayAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST: /api/events/{eventId}/enrol — PARTICIPANT ONLY
        [HttpPost("events/{eventId}/enrol")]
        public async Task<IActionResult> EnrolInEvent(int eventId, [FromBody] EnrolmentDto enrolmentDto)
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            if (userId == null)
                return Unauthorized(new { message = "You must be logged in." });

            if (!SessionHelper.IsInRole(HttpContext, "Participant"))
                return StatusCode(403, new { message = "Only Participants can enrol in events." });

            // Find the participant profile
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.UserID == userId.Value);

            if (participant == null)
                return StatusCode(403, new { message = "Participant profile not found." });

            // Check the event exists
            var ev = await _context.Events
                .Include(e => e.Enrolments)
                .FirstOrDefaultAsync(e => e.EventID == eventId);

            if (ev == null)
                return NotFound(new { message = "Event not found." });

            // Check event status
            if (ev.Status != "Open")
                return BadRequest(new { message = "This event is not open for enrolment." });

            // Check event is not full
            if (ev.Enrolments.Count >= ev.MaxParticipants)
                return BadRequest(new { message = "This event is full." });

            // Check the category belongs to this event
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryID == enrolmentDto.CategoryID && c.EventID == eventId);

            if (category == null)
                return BadRequest(new { message = "Category not found for this event." });

            // Check the participant isn't already enrolled
            var existing = await _context.Enrolments
                .FirstOrDefaultAsync(e => e.ParticipantID == participant.ParticipantID
                                       && e.EventID == eventId);

            if (existing != null)
                return BadRequest(new { message = "You are already enrolled in this event." });

            // Generate unique RaceNumber
            var raceNumber = await GenerateRaceNumberAsync();

            var enrolment = new Enrolment
            {
                ParticipantID = participant.ParticipantID,
                EventID = eventId,
                CategoryID = enrolmentDto.CategoryID,
                EnrolmentDate = DateTime.UtcNow,
                Status = "Registered",
                RaceNumber = raceNumber
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyEnrolments), null, new
            {
                enrolment.EnrolmentID,
                enrolment.EventID,
                EventName = ev.EventName,
                enrolment.CategoryID,
                CategoryName = category.CategoryName,
                enrolment.RaceNumber,
                enrolment.Status,
                enrolment.EnrolmentDate
            });
        }

        // GET: /api/participants/enrolments — PARTICIPANT ONLY
        [HttpGet("participants/enrolments")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            if (userId == null)
                return Unauthorized(new { message = "You must be logged in." });

            if (!SessionHelper.IsInRole(HttpContext, "Participant"))
                return StatusCode(403, new { message = "Only Participants can view their enrolments." });

            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.UserID == userId.Value);

            if (participant == null)
                return StatusCode(403, new { message = "Participant profile not found." });

            var enrolments = await _context.Enrolments
                .Where(e => e.ParticipantID == participant.ParticipantID)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Select(e => new
                {
                    e.EnrolmentID,
                    EventName = e.Event != null ? e.Event.EventName : null,
                    EventDate = e.Event != null ? e.Event.EventDateAndTime : (DateTime?)null,
                    CategoryName = e.Category != null ? e.Category.CategoryName : null,
                    e.RaceNumber,
                    e.Status,
                    e.EnrolmentDate
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: /api/events/{eventId}/enrolments — ORGANISER (OWNER) ONLY
        [HttpGet("events/{eventId}/enrolments")]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            if (userId == null)
                return Unauthorized(new { message = "You must be logged in." });

            if (!SessionHelper.IsInRole(HttpContext, "Organiser"))
                return StatusCode(403, new { message = "Only Organisers can view event enrolments." });

            var ev = await _context.Events
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventID == eventId);

            if (ev == null)
                return NotFound(new { message = "Event not found." });

            if (ev.Organiser == null || ev.Organiser.UserID != userId.Value)
                return StatusCode(403, new { message = "You can only view enrolments for your own events." });

            var enrolments = await _context.Enrolments
                .Where(e => e.EventID == eventId)
                .Include(e => e.Participant).ThenInclude(p => p!.User)
                .Include(e => e.Category)
                .Select(e => new
                {
                    e.EnrolmentID,
                    ParticipantName = e.Participant != null && e.Participant.User != null
                        ? e.Participant.User.FullName : null,
                    CategoryName = e.Category != null ? e.Category.CategoryName : null,
                    e.RaceNumber,
                    e.Status,
                    e.EnrolmentDate
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // PUT: /api/enrolments/{id}/withdraw — PARTICIPANT (OWN) ONLY
        [HttpPut("enrolments/{id}/withdraw")]
        public async Task<IActionResult> WithdrawFromEvent(int id)
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            if (userId == null)
                return Unauthorized(new { message = "You must be logged in." });

            if (!SessionHelper.IsInRole(HttpContext, "Participant"))
                return StatusCode(403, new { message = "Only Participants can withdraw from events." });

            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.UserID == userId.Value);

            if (participant == null)
                return StatusCode(403, new { message = "Participant profile not found." });

            var enrolment = await _context.Enrolments
                .FirstOrDefaultAsync(e => e.EnrolmentID == id);

            if (enrolment == null)
                return NotFound(new { message = "Enrolment not found." });

            if (enrolment.ParticipantID != participant.ParticipantID)
                return StatusCode(403, new { message = "You can only withdraw from your own enrolments." });

            if (enrolment.Status == "Withdrawn")
                return BadRequest(new { message = "You have already withdrawn from this event." });

            enrolment.Status = "Withdrawn";
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Withdrawn successfully.",
                enrolment.EnrolmentID,
                enrolment.Status
            });
        }

        // Helper: Generate unique race number
        private async Task<int> GenerateRaceNumberAsync()
        {
            var maxRaceNumber = await _context.Enrolments
                .Select(e => (int?)e.RaceNumber)
                .MaxAsync();

            return (maxRaceNumber ?? 100) + 1;
        }
    }
}