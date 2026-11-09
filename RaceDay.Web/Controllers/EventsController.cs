using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;
using RaceDay.Web.Models.ViewModels;
using System.Text.Json;

namespace RaceDay.Web.Controllers
{
    public class EventsController : Controller
    {
        private readonly IApiService _apiService;
        private readonly JsonSerializerOptions _jsonOptions;

        public EventsController(IApiService apiService)
        {
            _apiService = apiService;
            // Centralise configuration to ensure consistent case-insensitive mapping across all actions
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // GET: /Events
        public async Task<IActionResult> Index()
        {
            var response = await _apiService.GetAsync("/api/events");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var events = JsonSerializer.Deserialize<List<EventViewModel>>(json, _jsonOptions);
                return View(events);
            }
            return View(new List<EventViewModel>());
        }

        // GET: /Events/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var response = await _apiService.GetAsync($"/api/events/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                var viewModel = new EventDetailsViewModel
                {
                    Event = new EventViewModel
                    {
                        EventID = root.TryGetProperty("eventID", out var idProp) ? idProp.GetInt32() : root.TryGetProperty("eventId", out var idProp2) ? idProp2.GetInt32() : 0,
                        EventName = root.TryGetProperty("eventName", out var nameProp) ? nameProp.GetString() ?? "" : "",
                        EventDateAndTime = root.TryGetProperty("eventDateAndTime", out var dateProp) ? dateProp.GetDateTime() : DateTime.MinValue,
                        Location = root.TryGetProperty("location", out var locProp) ? locProp.GetString() ?? "" : "",
                        Distance = root.TryGetProperty("distance", out var distProp) ? distProp.GetDecimal() : 0,
                        EventType = root.TryGetProperty("eventType", out var typeProp) ? typeProp.GetString() ?? "" : "",
                        Status = root.TryGetProperty("status", out var statusProp) ? statusProp.GetString() ?? "" : ""
                    }
                };

                // Map Categories
                if (root.TryGetProperty("categories", out var categoriesArray) && categoriesArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var cat in categoriesArray.EnumerateArray())
                    {
                        viewModel.Categories.Add(new CategoryViewModel
                        {
                            CategoryID = cat.TryGetProperty("categoryID", out var catId) ? catId.GetInt32() : cat.TryGetProperty("categoryId", out var catId2) ? catId2.GetInt32() : 0,
                            CategoryName = cat.TryGetProperty("categoryName", out var catName) ? catName.GetString() ?? "" : "",
                            MinAge = cat.TryGetProperty("minAge", out var minAge) ? minAge.GetInt32() : 0,
                            MaxAge = cat.TryGetProperty("maxAge", out var maxAge) ? maxAge.GetInt32() : 0,
                            Gender = cat.TryGetProperty("gender", out var gender) ? gender.GetString() ?? "" : "",
                            EntryFee = cat.TryGetProperty("entryFee", out var fee) ? fee.GetDecimal() : 0
                        });
                    }
                }

                return View(viewModel);
            }
            return NotFound();
        }

        // POST: /Events/Enrol
        [HttpPost]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {
            // Check if user is logged in
            if (!Request.Cookies.ContainsKey(".RaceDay.Session"))
            {
                return RedirectToAction("Login", "Account");
            }

            var enrolDto = new { CategoryID = categoryId };
            var response = await _apiService.PostAsync($"/api/events/{eventId}/enrol", enrolDto);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Successfully enrolled in the event!";
                return RedirectToAction("MyEnrolments", "Participant");
            }

            // Failure block: Extract the API error message and store it in TempData
            var errorJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(errorJson);
            var errorMessage = doc.RootElement.TryGetProperty("message", out var msg) ? msg.GetString() : "Enrolment failed.";
            TempData["ErrorMessage"] = errorMessage;

            // Redirect back to the details page of the specific event
            return RedirectToAction("Details", new { id = eventId });
        }
    }
}
