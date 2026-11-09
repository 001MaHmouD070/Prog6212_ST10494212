using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;
using System.Text.Json;

namespace RaceDay.Web.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly IApiService _apiService;

        public ParticipantController(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> MyEnrolments()
        {
            // Check if logged in
            if (!Request.Cookies.ContainsKey(".RaceDay.Session"))
            {
                return RedirectToAction("Login", "Account");
            }

            // Fetch enrolments from the API
            var response = await _apiService.GetAsync("/api/participants/enrolments");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                // Pass the JSON directly to the view for simplicity, or map it. 
                // We will use ViewBag for this quick demonstration.
                ViewBag.EnrolmentsJson = json;
                return View();
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                // If an Organiser clicks this, redirect them to the home page
                return RedirectToAction("Index", "Home");
            }

            ViewBag.EnrolmentsJson = "[]";
            return View();
        }
    }
}