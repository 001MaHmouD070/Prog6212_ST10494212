using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Raceday.API.DTOs;
using RacedayAPI.Data;
using System.Net.Http.Json;

namespace Raceday.API.Tests.IntegrationTests
{
    /// <summary>
    /// Custom factory that replaces the real SQL Server database with an 
    /// in-memory database for isolated, fast, repeatable tests.
    /// </summary>
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<RaceDayDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<RaceDayDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"RaceDayTestDb_{Guid.NewGuid()}");
                });

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
                db.Database.EnsureCreated();
            });
        }
    }

    /// <summary>
    /// Helper methods for common test operations.
    /// </summary>
    public static class TestHelpers
    {
        /// <summary>
        /// Registers a user with the given role and returns the HttpClient (ready to use).
        /// </summary>
        public static async Task<HttpClient> RegisterAndLoginAsync(
            HttpClient client,
            string email,
            string role,
            string password = "password123")
        {
            // Register
            var registerDto = new RegisterDto
            {
                Email = email,
                Password = password,
                FullName = $"Test {role}",
                Role = role,
                CompanyName = role == "Organiser" ? "Test Co" : null,
                PhoneNumber = role == "Organiser" ? "0821234567" : null,
                DateOfBirth = role == "Participant" ? new DateTime(1990, 1, 1) : null,
                Gender = role == "Participant" ? "Male" : null,
                EmergencyContact = role == "Participant" ? "0821111111" : null
            };

            await client.PostAsJsonAsync("/api/auth/register", registerDto);

            // Login
            var loginDto = new LoginDto { Email = email, Password = password };
            await client.PostAsJsonAsync("/api/auth/login", loginDto);

            return client;
        }
    }
}