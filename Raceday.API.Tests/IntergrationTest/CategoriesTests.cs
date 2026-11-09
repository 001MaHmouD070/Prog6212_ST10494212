using FluentAssertions;
using Raceday.API.DTOs;
using System.Net;
using System.Net.Http.Json;

namespace Raceday.API.Tests.IntegrationTests
{
    public class CategoriesTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public CategoriesTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        private static CategoryDto CreateSampleCategoryDto()
        {
            return new CategoryDto
            {
                CategoryName = "Senior Men 18-39",
                MinAge = 18,
                MaxAge = 39,
                Gender = "Male",
                EntryFee = 250.00m
            };
        }

        [Fact]
        public async Task CreateCategory_AsOrganiser_Returns201()
        {
            await TestHelpers.RegisterAndLoginAsync(_client, "catorg@test.com", "Organiser");

            var response = await _client.PostAsJsonAsync("/api/events/1/categories", CreateSampleCategoryDto());

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task CreateCategory_AsParticipant_Returns403()
        {
            await TestHelpers.RegisterAndLoginAsync(_client, "catpart@test.com", "Participant");

            var response = await _client.PostAsJsonAsync("/api/events/1/categories", CreateSampleCategoryDto());

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateCategory_WithInvalidGender_Returns400()
        {
            await TestHelpers.RegisterAndLoginAsync(_client, "catgen@test.com", "Organiser");

            var dto = CreateSampleCategoryDto();
            dto.Gender = "Alien";

            var response = await _client.PostAsJsonAsync("/api/events/1/categories", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateCategory_WithInvalidAgeRange_Returns400()
        {
            await TestHelpers.RegisterAndLoginAsync(_client, "catage@test.com", "Organiser");

            var dto = CreateSampleCategoryDto();
            dto.MinAge = 50;
            dto.MaxAge = 30;

            var response = await _client.PostAsJsonAsync("/api/events/1/categories", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}