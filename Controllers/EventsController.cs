using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Raceday.API.DTOs;
using Raceday.API.Services;
using RacedayAPI.Data;
using RacedayAPI.Models;
using RacedayAPI.Services;

namespace RacedayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly BlobService _blobService;

        public EventsController(RaceDayDbContext context, BlobService blobService) 
        {
            _context = context;
            _blobService = blobService;
        }

        // GET: /api/events — Public
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.Events
                .Include(e => e.Organiser)
                .Include(e => e.Categories)
                .Select(e => new
                {
                    e.EventID,
                    e.EventName,
                    e.EventDateAndTime,
                    e.Location,
                    e.Distance,
                    e.EventType,
                    e.Status,
                    e.MaxParticipants,
                    e.CreatedAt,
                    Organiser = e.Organiser == null ? null : new
                    {
                        e.Organiser.OrganiserID,
                        e.Organiser.CompanyName
                    },
                    CategoryCount = e.Categories.Count
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: /api/events/{id} — Public
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Organiser)
                .Include(e => e.Categories)
                .Select(e => new
                {
                    e.EventID,
                    e.EventName,
                    e.EventDateAndTime,
                    e.Location,
                    e.Distance,
                    e.EventType,
                    e.Status,
                    e.MaxParticipants,
                    e.CreatedAt,
                    Organiser = e.Organiser == null ? null : new
                    {
                        e.Organiser.OrganiserID,
                        e.Organiser.CompanyName,
                        e.Organiser.PhoneNumber
                    },
                    Categories = e.Categories.Select(c => new
                    {
                        c.CategoryID,
                        c.CategoryName,
                        c.MinAge,
                        c.MaxAge,
                        c.Gender,
                        c.EntryFee
                    })
                })
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (ev == null)
                return NotFound(new { message = "Event not found." });

            return Ok(ev);
        }

        // POST: /api/events — ORGANISER ONLY
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] EventDto eventDto)
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            if (userId == null) return Unauthorized(new { message = "You must be logged in." });
            if (!SessionHelper.IsInRole(HttpContext, "Organiser")) return StatusCode(403, new { message = "Only Organisers can create events." });

            var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserID == userId.Value);
            if (organiser == null) return StatusCode(403, new { message = "Organiser profile not found." });

            var newEvent = new Event
            {
                OrganiserID = organiser.OrganiserID,
                EventName = eventDto.EventName,
                EventDateAndTime = eventDto.EventDateAndTime,
                Location = eventDto.Location,
                Distance = eventDto.Distance,
                EventType = eventDto.EventType,
                Status = eventDto.Status,
                MaxParticipants = eventDto.MaxParticipants,
                CreatedAt = DateTime.UtcNow
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            var response = new
            {
                newEvent.EventID,
                newEvent.EventName,
                newEvent.EventDateAndTime,
                newEvent.Location,
                newEvent.Distance,
                newEvent.EventType,
                newEvent.Status,
                newEvent.MaxParticipants,
                newEvent.CreatedAt,
                Organiser = new
                {
                    organiser.OrganiserID,
                    organiser.CompanyName,
                    organiser.PhoneNumber
                },
                Categories = Array.Empty<object>()
            };

            return CreatedAtAction(nameof(GetEventById), new { id = newEvent.EventID }, response);
        }

        // PUT: /api/events/{id} — ORGANISER (OWNER) ONLY
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] EventDto eventDto)
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            if (userId == null) return Unauthorized(new { message = "You must be logged in." });
            if (!SessionHelper.IsInRole(HttpContext, "Organiser")) return StatusCode(403, new { message = "Only Organisers can update events." });

            var ev = await _context.Events.Include(e => e.Organiser).FirstOrDefaultAsync(e => e.EventID == id);
            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.Organiser == null || ev.Organiser.UserID != userId.Value) return StatusCode(403, new { message = "You can only edit your own events." });

            ev.EventName = eventDto.EventName;
            ev.EventDateAndTime = eventDto.EventDateAndTime;
            ev.Location = eventDto.Location;
            ev.Distance = eventDto.Distance;
            ev.EventType = eventDto.EventType;
            ev.Status = eventDto.Status;
            ev.MaxParticipants = eventDto.MaxParticipants;

            await _context.SaveChangesAsync();

            return Ok(new { ev.EventID, ev.EventName, ev.EventDateAndTime, ev.Location, ev.Distance, ev.EventType, ev.Status, ev.MaxParticipants, ev.CreatedAt, OrganiserID = ev.Organiser?.OrganiserID, CompanyName = ev.Organiser?.CompanyName });
        }

        // DELETE: /api/events/{id} — ORGANISER (OWNER) ONLY
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            if (userId == null) return Unauthorized(new { message = "You must be logged in." });
            if (!SessionHelper.IsInRole(HttpContext, "Organiser")) return StatusCode(403, new { message = "Only Organisers can delete events." });

            var ev = await _context.Events.Include(e => e.Organiser).FirstOrDefaultAsync(e => e.EventID == id);
            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.Organiser == null || ev.Organiser.UserID != userId.Value) return StatusCode(403, new { message = "You can only delete your own events." });

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: /api/events/{id}/upload-image
        [HttpPost("{id}/upload-image")]
        public async Task<IActionResult> UploadEventBanner(int id, IFormFile file)
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            if (userId == null) return Unauthorized(new { message = "You must be logged in." });
            if (!SessionHelper.IsInRole(HttpContext, "Organiser")) return StatusCode(403, new { message = "Only Organisers can upload images." });

            var ev = await _context.Events.Include(e => e.Organiser).FirstOrDefaultAsync(e => e.EventID == id);
            if (ev == null) return NotFound(new { message = "Event not found." });

            if (ev.Organiser?.UserID != userId.Value) return StatusCode(403, new { message = "You can only upload images for your own events." });

            if (file == null || file.Length == 0) return BadRequest(new { message = "No file uploaded." });

            try
            {
                var imageUrl = await _blobService.UploadFileAsync(file);
                return Ok(new { imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to upload image.", error = ex.Message });
            }
        }
    }
}