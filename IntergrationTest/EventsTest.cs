using FluentAssertions;
using Raceday.API.DTOs;
using System.Net;
using System.Net.Http.Json;

namespace Raceday.API.Tests.IntegrationTests
{
    public class EventsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public EventsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        private static EventDto CreateSampleEventDto()
        {
            return new EventDto
            {
                EventName = "Test Marathon",
                EventDateAndTime = new DateTime(2026, 12, 1, 6, 0, 0),
                Location = "Test Location",
                Distance = 42.20m,
                EventType = "Road Running",
                Status = "Open",
                MaxParticipants = 5000
            };
        }

        [Fact]
        public async Task GetAllEvents_WithoutLogin_Returns200()
        {
            var response = await _client.GetAsync("/api/events");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateEvent_WithoutLogin_Returns401()
        {
            var response = await _client.PostAsJsonAsync("/api/events", CreateSampleEventDto());

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateEvent_AsOrganiser_Returns201()
        {
            await TestHelpers.RegisterAndLoginAsync(_client, "evtorg@test.com", "Organiser");

            var response = await _client.PostAsJsonAsync("/api/events", CreateSampleEventDto());

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task CreateEvent_AsParticipant_Returns403()
        {
            await TestHelpers.RegisterAndLoginAsync(_client, "evtpart@test.com", "Participant");

            var response = await _client.PostAsJsonAsync("/api/events", CreateSampleEventDto());

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetEventById_AfterCreate_Returns200()
        {
            await TestHelpers.RegisterAndLoginAsync(_client, "getorg@test.com", "Organiser");
            var createResponse = await _client.PostAsJsonAsync("/api/events", CreateSampleEventDto());
            var created = await createResponse.Content.ReadFromJsonAsync<dynamic>();

            var response = await _client.GetAsync($"/api/events/1");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}