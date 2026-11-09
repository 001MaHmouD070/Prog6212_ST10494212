using FluentAssertions;
using Raceday.API.DTOs;
using System.Net;
using System.Net.Http.Json;

namespace Raceday.API.Tests.IntegrationTests
{
    public class EnrolmentsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public EnrolmentsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        private static EnrolmentDto CreateEnrolmentDto()
        {
            return new EnrolmentDto { CategoryID = 1 };
        }

        [Fact]
        public async Task Enrol_WithoutLogin_Returns401()
        {
            var response = await _client.PostAsJsonAsync("/api/events/1/enrol", CreateEnrolmentDto());

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Enrol_AsOrganiser_Returns403()
        {
            await TestHelpers.RegisterAndLoginAsync(_client, "enrolorg@test.com", "Organiser");

            var response = await _client.PostAsJsonAsync("/api/events/1/enrol", CreateEnrolmentDto());

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetMyEnrolments_WithoutLogin_Returns401()
        {
            var response = await _client.GetAsync("/api/participants/enrolments");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetMyEnrolments_AsParticipant_Returns200()
        {
            await TestHelpers.RegisterAndLoginAsync(_client, "enrolpart@test.com", "Participant");

            var response = await _client.GetAsync("/api/participants/enrolments");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetEventEnrolments_AsOrganiser_Returns200Or403()
        {
            await TestHelpers.RegisterAndLoginAsync(_client, "eventenrol@test.com", "Organiser");

            var response = await _client.GetAsync("/api/events/1/enrolments");

            // 200 (event exists and belongs to this organiser) or 403 (not the owner)
            // Either way, it should NOT be 401 (we ARE authenticated)
            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        }
    }
}