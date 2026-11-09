using FluentAssertions;
using Raceday.API.DTOs;
using System.Net;
using System.Net.Http.Json;

namespace Raceday.API.Tests.IntegrationTests
{
    public class ResultsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ResultsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        private static ResultDto CreateSampleResultDto()
        {
            return new ResultDto
            {
                EnrolmentID = 1,
                FinishTime = TimeSpan.FromMinutes(165),
                Position = 15,
                IsDisqualified = false,
                Notes = "Test result"
            };
        }

        [Fact]
        public async Task CaptureResult_WithoutLogin_Returns401()
        {
            var response = await _client.PostAsJsonAsync("/api/events/1/results", CreateSampleResultDto());

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CaptureResult_AsParticipant_Returns403()
        {
            await TestHelpers.RegisterAndLoginAsync(_client, "respart@test.com", "Participant");

            var response = await _client.PostAsJsonAsync("/api/events/1/results", CreateSampleResultDto());

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetMyResults_WithoutLogin_Returns401()
        {
            var response = await _client.GetAsync("/api/participants/results");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetMyResults_AsParticipant_Returns200()
        {
            await TestHelpers.RegisterAndLoginAsync(_client, "myres@test.com", "Participant");

            var response = await _client.GetAsync("/api/participants/results");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}