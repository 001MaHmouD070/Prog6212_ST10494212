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
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: /api/events/{eventId}/categories — Public
        [HttpGet("events/{eventId}/categories")]
        public async Task<IActionResult> GetCategoriesForEvent(int eventId)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null)
                return NotFound(new { message = "Event not found." });

            var categories = await _context.Categories
                .Where(c => c.EventID == eventId)
                .Select(c => new
                {
                    c.CategoryID,
                    c.CategoryName,
                    c.MinAge,
                    c.MaxAge,
                    c.Gender,
                    c.EntryFee
                })
                .ToListAsync();

            return Ok(categories);
        }

        // POST: /api/events/{eventId}/categories — ORGANISER (OWNER) ONLY
        [HttpPost("events/{eventId}/categories")]
        public async Task<IActionResult> CreateCategory(int eventId, [FromBody] CategoryDto categoryDto)
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            if (userId == null)
                return Unauthorized(new { message = "You must be logged in." });

            if (!SessionHelper.IsInRole(HttpContext, "Organiser"))
                return StatusCode(403, new { message = "Only Organisers can create categories." });

            var ev = await _context.Events
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventID == eventId);

            if (ev == null)
                return NotFound(new { message = "Event not found." });

            if (ev.Organiser == null || ev.Organiser.UserID != userId.Value)
                return StatusCode(403, new { message = "You can only add categories to your own events." });

            // Validate gender
            if (categoryDto.Gender != "Male" && categoryDto.Gender != "Female" && categoryDto.Gender != "Open")
                return BadRequest(new { message = "Gender must be Male, Female, or Open." });

            if (categoryDto.MaxAge < categoryDto.MinAge)
                return BadRequest(new { message = "MaxAge must be greater than or equal to MinAge." });

            var newCategory = new Category
            {
                EventID = eventId,
                CategoryName = categoryDto.CategoryName,
                MinAge = categoryDto.MinAge,
                MaxAge = categoryDto.MaxAge,
                Gender = categoryDto.Gender,
                EntryFee = categoryDto.EntryFee
            };

            _context.Categories.Add(newCategory);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoriesForEvent), new { eventId = eventId }, new
            {
                newCategory.CategoryID,
                newCategory.CategoryName,
                newCategory.MinAge,
                newCategory.MaxAge,
                newCategory.Gender,
                newCategory.EntryFee
            });
        }

        // PUT: /api/categories/{id} — ORGANISER (OWNER) ONLY
        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryDto categoryDto)
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            if (userId == null)
                return Unauthorized(new { message = "You must be logged in." });

            if (!SessionHelper.IsInRole(HttpContext, "Organiser"))
                return StatusCode(403, new { message = "Only Organisers can update categories." });

            var category = await _context.Categories
                .Include(c => c.Event)
                    .ThenInclude(e => e!.Organiser)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            if (category.Event?.Organiser == null || category.Event.Organiser.UserID != userId.Value)
                return StatusCode(403, new { message = "You can only edit categories of your own events." });

            if (categoryDto.Gender != "Male" && categoryDto.Gender != "Female" && categoryDto.Gender != "Open")
                return BadRequest(new { message = "Gender must be Male, Female, or Open." });

            if (categoryDto.MaxAge < categoryDto.MinAge)
                return BadRequest(new { message = "MaxAge must be greater than or equal to MinAge." });

            category.CategoryName = categoryDto.CategoryName;
            category.MinAge = categoryDto.MinAge;
            category.MaxAge = categoryDto.MaxAge;
            category.Gender = categoryDto.Gender;
            category.EntryFee = categoryDto.EntryFee;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                category.CategoryID,
                category.CategoryName,
                category.MinAge,
                category.MaxAge,
                category.Gender,
                category.EntryFee
            });
        }

        // DELETE: /api/categories/{id} — ORGANISER (OWNER) ONLY
        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            if (userId == null)
                return Unauthorized(new { message = "You must be logged in." });

            if (!SessionHelper.IsInRole(HttpContext, "Organiser"))
                return StatusCode(403, new { message = "Only Organisers can delete categories." });

            var category = await _context.Categories
                .Include(c => c.Event)
                    .ThenInclude(e => e!.Organiser)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            if (category.Event?.Organiser == null || category.Event.Organiser.UserID != userId.Value)
                return StatusCode(403, new { message = "You can only delete categories of your own events." });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}