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
    public class AuctionControllerTests : IClassFixture<CustomWebAppFactory>, IAsyncLifetime
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
        
        public Task DisposeAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
            DbHelper.ReinitDbForTests(db);
            return Task.CompletedTask;
        }

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

        [Fact]
        public async Task GetAuctions_ShouldReturn3Auctions()
        {
            // arrange

            //act

            var response = await _httpClient.GetFromJsonAsync<List<AuctionDto>>("api/auctions");

            // assert

            Assert.Equal(3, response.Count);
        }

        [Fact]
        public async Task GetAuctionById_WithValidId_ShouldReturnAuction()
        {
            // arrange

            // act

            var response = await _httpClient.GetFromJsonAsync<AuctionDto>($"api/auctions/{GT_ID}");

            Assert.Equal("GT", response.Model);
        }

        [Fact]
        public async Task GetAuctionById_WithInvalidId_ShouldReturn404NotFound()
        {
            // arrange

            // act

            var response = await _httpClient.GetAsync($"api/auctions/{new Guid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetAuctionById_WithInvalidGuid_ShouldReturn400BadRequest()
        {
            // arrange

            // act

            var response = await _httpClient.GetAsync($"api/auctions/not-a-valid-guid");
            
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateAuction_WithNoAuth_ShouldReturn401Unauth()
        {
            // arrange

            CreateAuctionDto auction = new CreateAuctionDto { Make = "test" };

            // act

            var response = await _httpClient.PostAsJsonAsync($"api/auctions", auction);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

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

    }
}
