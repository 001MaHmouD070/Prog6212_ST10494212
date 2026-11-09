using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;
using RaceDay.Web.Models.ViewModels;
using System.Text.Json;

namespace RaceDay.Web.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly IApiService _apiService;

        public OrganiserController(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!Request.Cookies.ContainsKey(".RaceDay.Session"))
            {
                return RedirectToAction("Login", "Account");
            }

            var profileResponse = await _apiService.GetAsync("/api/organisers/profile");

            if (profileResponse.StatusCode == System.Net.HttpStatusCode.Forbidden ||
                profileResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Index", "Home");
            }

            var eventsResponse = await _apiService.GetAsync("/api/events");

            if (eventsResponse.IsSuccessStatusCode)
            {
                var json = await eventsResponse.Content.ReadAsStringAsync();
                var allEvents = JsonSerializer.Deserialize<List<EventViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(allEvents);
            }

            return View(new List<EventViewModel>());
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            if (!Request.Cookies.ContainsKey(".RaceDay.Session")) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent(EventViewModel model)
        {
            if (!Request.Cookies.ContainsKey(".RaceDay.Session")) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(model);

            var eventData = new
            {
                EventName = model.EventName,
                EventDateAndTime = model.EventDateAndTime,
                Location = model.Location,
                Distance = model.Distance,
                EventType = model.EventType,
                Status = "Open",
                MaxParticipants = model.MaxParticipants
            };

            var response = await _apiService.PostAsync("/api/events", eventData);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction("Dashboard");
            }

            var errorJson = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"API Error: {errorJson}");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ManageEvent(int id)
        {
            if (!Request.Cookies.ContainsKey(".RaceDay.Session")) return RedirectToAction("Login", "Account");

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
                        EventID = root.TryGetProperty("eventID", out var idProp) ? idProp.GetInt32() :
                                  root.TryGetProperty("eventId", out var idProp2) ? idProp2.GetInt32() : 0,
                        EventName = root.TryGetProperty("eventName", out var nameProp) ? nameProp.GetString() ?? "" : "",
                        EventDateAndTime = root.TryGetProperty("eventDateAndTime", out var dateProp) ? dateProp.GetDateTime() : DateTime.MinValue,
                        Location = root.TryGetProperty("location", out var locProp) ? locProp.GetString() ?? "" : "",
                        Distance = root.TryGetProperty("distance", out var distProp) ? distProp.GetDecimal() : 0,
                        EventType = root.TryGetProperty("eventType", out var typeProp) ? typeProp.GetString() ?? "" : "",
                        Status = root.TryGetProperty("status", out var statusProp) ? statusProp.GetString() ?? "" : ""
                    }
                };

                if (root.TryGetProperty("categories", out var categoriesArray) && categoriesArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var cat in categoriesArray.EnumerateArray())
                    {
                        viewModel.Categories.Add(new CategoryViewModel
                        {
                            CategoryID = cat.TryGetProperty("categoryID", out var catId) ? catId.GetInt32() :
                                         cat.TryGetProperty("categoryId", out var catId2) ? catId2.GetInt32() : 0,
                            CategoryName = cat.TryGetProperty("categoryName", out var catName) ? catName.GetString() ?? "" : "",
                            MinAge = cat.TryGetProperty("minAge", out var minAge) ? minAge.GetInt32() : 0,
                            MaxAge = cat.TryGetProperty("maxAge", out var maxAge) ? maxAge.GetInt32() : 0,
                            Gender = cat.TryGetProperty("gender", out var gender) ? gender.GetString() ?? "" : "",
                            EntryFee = cat.TryGetProperty("entryFee", out var fee) ? fee.GetDecimal() : 0
                        });
                    }
                }

                var enrolmentsResponse = await _apiService.GetAsync($"/api/events/{id}/enrolments");
                if (enrolmentsResponse.IsSuccessStatusCode)
                {
                    var enrolmentsJson = await enrolmentsResponse.Content.ReadAsStringAsync();
                    var enrolmentsList = JsonSerializer.Deserialize<List<EnrolmentViewModel>>(enrolmentsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (enrolmentsList != null)
                    {
                        viewModel.Enrolments = enrolmentsList;
                    }
                }

                return View(viewModel);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(int eventId, CategoryViewModel model)
        {
            if (!Request.Cookies.ContainsKey(".RaceDay.Session")) return RedirectToAction("Login", "Account");

            var categoryData = new
            {
                CategoryName = model.CategoryName,
                MinAge = model.MinAge,
                MaxAge = model.MaxAge,
                Gender = model.Gender,
                EntryFee = model.EntryFee
            };

            var response = await _apiService.PostAsync($"/api/events/{eventId}/categories", categoryData);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Category added successfully!";
            }
            else
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Failed to add category: {errorJson}";
            }

            return RedirectToAction("ManageEvent", new { id = eventId });
        }

        [HttpPost]
        public async Task<IActionResult> CaptureResult(int eventId, int enrolmentId, string finishTime, int position)
        {
            if (!Request.Cookies.ContainsKey(".RaceDay.Session")) return RedirectToAction("Login", "Account");

            if (finishTime.Length == 5) finishTime += ":00";

            var resultData = new
            {
                EnrolmentID = enrolmentId,
                FinishTime = finishTime,
                Position = position,
                IsDisqualified = false,
                Notes = "Captured via MVC"
            };

            var response = await _apiService.PostAsync($"/api/events/{eventId}/results", resultData);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Result captured successfully!";
            }
            else
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Failed to capture result: {errorJson}";
            }

            return RedirectToAction("ManageEvent", new { id = eventId });
        }

        // POST: /Organiser/UploadImage
        [HttpPost]
        public async Task<IActionResult> UploadImage(int eventId, IFormFile file)
        {
            if (!Request.Cookies.ContainsKey(".RaceDay.Session")) return RedirectToAction("Login", "Account");

            // ⬇️ ADDED THE FILE NULL CHECK HERE ⬇️
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select an image file before clicking upload.";
                return RedirectToAction("ManageEvent", new { id = eventId });
            }
            // ⬆️ END OF CHECK ⬆️

            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream());
            content.Add(fileContent, "file", file.FileName);

            var response = await _apiService.PostAsync($"/api/events/{eventId}/upload-image", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Image uploaded to Azure Blob Storage successfully!";
            }
            else
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Upload failed: {errorJson}";
            }

            return RedirectToAction("ManageEvent", new { id = eventId });
        }
    }
}