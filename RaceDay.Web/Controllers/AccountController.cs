using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models.ViewModels;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IApiService _apiService;

        public AccountController(IApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Call the API login endpoint
            var response = await _apiService.PostAsync("/api/auth/login", model);

            if (response.IsSuccessStatusCode)
            {
                // CookieHandler automatically saved the session cookie to the browser
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Call the API register endpoint
            var response = await _apiService.PostAsync("/api/auth/register", model);

            if (response.IsSuccessStatusCode)
            {
                // Registration successful, now log them in
                var loginModel = new LoginViewModel { Email = model.Email, Password = model.Password };
                var loginResponse = await _apiService.PostAsync("/api/auth/login", loginModel);

                if (loginResponse.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            // Updated error handling block
            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"API Error: {errorContent}");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _apiService.PostAsync("/api/auth/logout", new { });
            Response.Cookies.Delete(".RaceDay.Session");
            return RedirectToAction("Index", "Home");
        }
    }
}