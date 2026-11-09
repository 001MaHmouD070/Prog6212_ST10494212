using FluentAssertions;
using Raceday.API.DTOs;
using System.Net;
using System.Net.Http.Json;

namespace Raceday.API.Tests.IntegrationTests
{
    public class AuthTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        // ============================================================
        // REGISTRATION TESTS
        // ============================================================

        [Fact]
        public async Task Register_WithValidOrganiserData_Returns201()
        {
            var registerDto = new RegisterDto
            {
                Email = "organiser1@test.com",
                Password = "password123",
                FullName = "Test Organiser",
                Role = "Organiser",
                CompanyName = "Test Co",
                PhoneNumber = "0821234567"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Register_WithValidParticipantData_Returns201()
        {
            var registerDto = new RegisterDto
            {
                Email = "participant1@test.com",
                Password = "password123",
                FullName = "Test Participant",
                Role = "Participant",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Male",
                EmergencyContact = "0821111111"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_Returns400()
        {
            var registerDto = new RegisterDto
            {
                Email = "duplicate@test.com",
                Password = "password123",
                FullName = "First User",
                Role = "Organiser",
                CompanyName = "Test Co",
                PhoneNumber = "0821234567"
            };

            // Register once
            await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            // Try to register again with same email
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_WithInvalidRole_Returns400()
        {
            var registerDto = new RegisterDto
            {
                Email = "invalidrole@test.com",
                Password = "password123",
                FullName = "Invalid Role",
                Role = "SuperAdmin"  // Not allowed
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ============================================================
        // LOGIN TESTS
        // ============================================================

        [Fact]
        public async Task Login_WithValidCredentials_Returns200()
        {
            // Register
            await TestHelpers.RegisterAndLoginAsync(_client, "login@test.com", "Organiser");

            // Login with valid credentials
            var loginDto = new LoginDto
            {
                Email = "login@test.com",
                Password = "password123"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Login_WithWrongPassword_Returns401()
        {
            // Register
            await TestHelpers.RegisterAndLoginAsync(_client, "wrongpass@test.com", "Organiser");

            // Try to login with wrong password
            var loginDto = new LoginDto
            {
                Email = "wrongpass@test.com",
                Password = "wrongPassword!"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // ============================================================
        // SESSION TESTS
        // ============================================================

        [Fact]
        public async Task GetCurrentUser_WithoutLogin_Returns401()
        {
            var response = await _client.GetAsync("/api/auth/me");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetCurrentUser_AfterLogin_Returns200()
        {
            await TestHelpers.RegisterAndLoginAsync(_client, "me@test.com", "Organiser");

            var response = await _client.GetAsync("/api/auth/me");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Logout_ClearsSession()
        {
            await TestHelpers.RegisterAndLoginAsync(_client, "logout@test.com", "Organiser");

            // Logout
            await _client.PostAsync("/api/auth/logout", null);

            // Try to access /me — should fail
            var response = await _client.GetAsync("/api/auth/me");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}