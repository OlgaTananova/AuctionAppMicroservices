using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Util;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AuctionService.IntegrationTests
{
    // Integration tests for the AuctionController using xUnit, IClassFixture for shared test context, and IAsyncLifetime for async setup/teardown.

    [Collection("SharedCollection")]
    public class AuctionControllerTests :  IAsyncLifetime
    {
        private readonly CustomWebAppFactory _factory;
        private readonly HttpClient _httpClient;
        private const string GT_ID = "afbee524-5972-4075-8800-7d1f9d7b0a0c";

        public AuctionControllerTests(CustomWebAppFactory factory)
        {
            _factory = factory;
            _httpClient = _factory.CreateClient();
        }
        public Task InitializeAsync() => Task.CompletedTask;

        // Async dispose method to reset the database using a helper method.
        public Task DisposeAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
            DbHelper.ReinitDbForTests(db);
            return Task.CompletedTask;
        }

        // Helper method to create a valid CreateAuctionDto.
        private CreateAuctionDto GetAuctionForCreate()
        {
            return new CreateAuctionDto
            {
                Make = "test",
                Model = "testModel",
                ImageUrl = "test",
                Milage = 10,
                Year = 10,
                ReservePrice = 10,
                Color = "test"
            };
        }

        // Test to verify GetAuctions returns 3 auctions.
        [Fact]
        public async Task GetAuctions_ShouldReturn3Auctions()
        {
            // arrange

            //act

            var response = await _httpClient.GetFromJsonAsync<List<AuctionDto>>("api/auctions");

            // assert

            Assert.Equal(3, response.Count);
        }

        // Test to verify GetAuctionById returns the correct auction for a valid ID.

        [Fact]
        public async Task GetAuctionById_WithValidId_ShouldReturnAuction()
        {
            // arrange

            // act

            var response = await _httpClient.GetFromJsonAsync<AuctionDto>($"api/auctions/{GT_ID}");

            Assert.Equal("GT", response.Model);
        }

        // Test to verify GetAuctionById returns 404 for an invalid ID.

        [Fact]
        public async Task GetAuctionById_WithInvalidId_ShouldReturn404NotFound()
        {
            // arrange

            // act

            var response = await _httpClient.GetAsync($"api/auctions/{new Guid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // Test to verify GetAuctionById returns 400 for an invalid GUID.

        [Fact]
        public async Task GetAuctionById_WithInvalidGuid_ShouldReturn400BadRequest()
        {
            // arrange

            // act

            var response = await _httpClient.GetAsync($"api/auctions/not-a-valid-guid");
            
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Test to verify CreateAuction returns 401 Unauthorized when no authentication is provided.

        [Fact]
        public async Task CreateAuction_WithNoAuth_ShouldReturn401Unauth()
        {
            // arrange

            CreateAuctionDto auction = new CreateAuctionDto { Make = "test" };

            // act

            var response = await _httpClient.PostAsJsonAsync($"api/auctions", auction);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // Test to verify CreateAuction returns 201 Created when valid authentication and data are provided.

        [Fact]
        public async Task CreateAuction_WithAuth_ShouldReturn201Created()
        {
            // arrange

            CreateAuctionDto auction = GetAuctionForCreate();
            _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

            // act

            var response = await _httpClient.PostAsJsonAsync($"api/auctions", auction);

            // assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            AuctionDto createdAucton = await response.Content.ReadFromJsonAsync<AuctionDto>();
            Assert.Equal("bob", createdAucton.Seller);
        }

        // Test to verify CreateAuction returns 400 Bad Request when invalid data is provided.

        [Fact]
        public async Task CreateAuction_WithInvalidCreateAuctionDto_ShouldReturn400()
        {
            // arrange

            CreateAuctionDto auction = GetAuctionForCreate();
            auction.Color = null;
            _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

            // act

            var response = await _httpClient.PostAsJsonAsync($"api/auctions", auction);

            // assert
            
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        }

        // Test to verify UpdateAuction returns 200 OK when valid data and user are provided.

        [Fact]
        public async Task UpdateAuction_WithValidUpdateDtoAndUser_ShouldReturn200()
        {
            // arrange

            _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));
            UpdateAuctionDto updateAuctionDto = new UpdateAuctionDto { Make = "FordUpdated" };

            // act

            var response = await _httpClient.PutAsJsonAsync($"api/auctions/{GT_ID}", updateAuctionDto);

            // assert
            var updatedAuction = await _httpClient.GetFromJsonAsync<AuctionDto>($"api/auctions/{GT_ID}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("FordUpdated", updatedAuction.Make);

        }

        // Test to verify UpdateAuction returns 403 Forbidden when an invalid user tries to update.

        [Fact]
        public async Task UpdateAuction_WithValidUpdateDtoAndInvalidUser_ShouldReturn403()
        {
            // arrange
            _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("alice"));
            UpdateAuctionDto updateAuctionDto = new UpdateAuctionDto { Make = "FordUpdated" };

            // act
            var response = await _httpClient.PutAsJsonAsync($"api/auctions/{GT_ID}", updateAuctionDto);

            // assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
          
        }

    }
}
