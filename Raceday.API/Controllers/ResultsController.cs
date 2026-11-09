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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST: /api/events/{eventId}/results — ORGANISER (OWNER) ONLY
        [HttpPost("events/{eventId}/results")]
        public async Task<IActionResult> CaptureResult(int eventId, [FromBody] ResultDto resultDto)
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            if (userId == null)
                return Unauthorized(new { message = "You must be logged in." });

            if (!SessionHelper.IsInRole(HttpContext, "Organiser"))
                return StatusCode(403, new { message = "Only Organisers can capture results." });

            // Verify the event exists and belongs to this organiser
            var ev = await _context.Events
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventID == eventId);

            if (ev == null)
                return NotFound(new { message = "Event not found." });

            if (ev.Organiser == null || ev.Organiser.UserID != userId.Value)
                return StatusCode(403, new { message = "You can only capture results for your own events." });

            // Verify the enrolment exists, belongs to this event, and status is Completed or Registered
            var enrolment = await _context.Enrolments
                .Include(e => e.Result)
                .FirstOrDefaultAsync(e => e.EnrolmentID == resultDto.EnrolmentID
                                       && e.EventID == eventId);

            if (enrolment == null)
                return BadRequest(new { message = "Enrolment not found for this event." });

            // Check if a result already exists for this enrolment
            if (enrolment.Result != null)
                return BadRequest(new { message = "A result already exists for this enrolment. Use PUT to update it." });

            // Create the result
            var result = new Result
            {
                EnrolmentID = resultDto.EnrolmentID,
                FinishTime = resultDto.FinishTime,
                Position = resultDto.Position,
                IsDisqualified = resultDto.IsDisqualified,
                Notes = resultDto.Notes
            };

            _context.Results.Add(result);

            // Mark the enrolment as Completed
            enrolment.Status = "Completed";

            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                result.ResultID,
                result.EnrolmentID,
                result.FinishTime,
                result.Position,
                result.IsDisqualified,
                result.Notes,
                EnrolmentStatus = enrolment.Status
            });
        }

        // GET: /api/participants/results — PARTICIPANT ONLY
        [HttpGet("participants/results")]
        public async Task<IActionResult> GetMyResults()
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            if (userId == null)
                return Unauthorized(new { message = "You must be logged in." });

            if (!SessionHelper.IsInRole(HttpContext, "Participant"))
                return StatusCode(403, new { message = "Only Participants can view their results." });

            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.UserID == userId.Value);

            if (participant == null)
                return StatusCode(403, new { message = "Participant profile not found." });

            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Event)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Category)
                .Where(r => r.Enrolment != null && r.Enrolment.ParticipantID == participant.ParticipantID)
                .Select(r => new
                {
                    r.ResultID,
                    EventName = r.Enrolment != null && r.Enrolment.Event != null
                        ? r.Enrolment.Event.EventName : null,
                    CategoryName = r.Enrolment != null && r.Enrolment.Category != null
                        ? r.Enrolment.Category.CategoryName : null,
                    r.FinishTime,
                    r.Position,
                    r.IsDisqualified,
                    r.Notes,
                    RaceNumber = r.Enrolment != null ? r.Enrolment.RaceNumber : 0
                })
                .ToListAsync();

            return Ok(results);
        }

        // GET: /api/events/{eventId}/results — ORGANISER (OWNER) ONLY
        [HttpGet("events/{eventId}/results")]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            if (userId == null)
                return Unauthorized(new { message = "You must be logged in." });

            if (!SessionHelper.IsInRole(HttpContext, "Organiser"))
                return StatusCode(403, new { message = "Only Organisers can view event results." });

            var ev = await _context.Events
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventID == eventId);

            if (ev == null)
                return NotFound(new { message = "Event not found." });

            if (ev.Organiser == null || ev.Organiser.UserID != userId.Value)
                return StatusCode(403, new { message = "You can only view results for your own events." });

            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Participant)
                        .ThenInclude(p => p!.User)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Category)
                .Where(r => r.Enrolment != null && r.Enrolment.EventID == eventId)
                .Select(r => new
                {
                    r.ResultID,
                    ParticipantName = r.Enrolment != null
                        && r.Enrolment.Participant != null
                        && r.Enrolment.Participant.User != null
                        ? r.Enrolment.Participant.User.FullName : null,
                    CategoryName = r.Enrolment != null && r.Enrolment.Category != null
                        ? r.Enrolment.Category.CategoryName : null,
                    RaceNumber = r.Enrolment != null ? r.Enrolment.RaceNumber : 0,
                    r.FinishTime,
                    r.Position,
                    r.IsDisqualified,
                    r.Notes
                })
                .OrderBy(r => r.Position)
                .ToListAsync();

            return Ok(results);
        }

        // PUT: /api/results/{id} — ORGANISER (OWNER) ONLY
        [HttpPut("results/{id}")]
        public async Task<IActionResult> UpdateResult(int id, [FromBody] ResultDto resultDto)
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            if (userId == null)
                return Unauthorized(new { message = "You must be logged in." });

            if (!SessionHelper.IsInRole(HttpContext, "Organiser"))
                return StatusCode(403, new { message = "Only Organisers can update results." });

            var result = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Event)
                        .ThenInclude(ev => ev!.Organiser)
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (result == null)
                return NotFound(new { message = "Result not found." });

            // Verify ownership through the chain: Result → Enrolment → Event → Organiser
            if (result.Enrolment?.Event?.Organiser == null
                || result.Enrolment.Event.Organiser.UserID != userId.Value)
                return StatusCode(403, new { message = "You can only edit results for your own events." });

            result.FinishTime = resultDto.FinishTime;
            result.Position = resultDto.Position;
            result.IsDisqualified = resultDto.IsDisqualified;
            result.Notes = resultDto.Notes;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                result.ResultID,
                result.EnrolmentID,
                result.FinishTime,
                result.Position,
                result.IsDisqualified,
                result.Notes
            });
        }
    }
}