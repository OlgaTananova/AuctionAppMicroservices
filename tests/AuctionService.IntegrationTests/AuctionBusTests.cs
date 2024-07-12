using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Util;
using Contracts;
using MassTransit.Testing;
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
    // Integration tests for AuctionBus using xUnit, IClassFixture for shared test context, and IAsyncLifetime for async setup/teardown.

    [Collection("SharedCollection")]
    public class AuctionBusTests : IAsyncLifetime
    {
        private readonly CustomWebAppFactory _factory;
        private readonly HttpClient _httpClient;
        private ITestHarness _testHarness;

        public AuctionBusTests(CustomWebAppFactory factory)
        {
            _factory = factory;
            _httpClient = _factory.CreateClient();
            _testHarness = _factory.Services.GetTestHarness();
        }
        public Task DisposeAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
            DbHelper.ReinitDbForTests(db);
            return Task.CompletedTask;
        }

        public Task InitializeAsync() => Task.CompletedTask;

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

        // Test to verify that creating an auction publishes an AuctionCreated event.

        [Fact]
        public async Task CreateAuction_WithValidObject_ShouldPublishAuctionCreated()
        {
            // Arrange: Prepare a valid auction and set authentication.
            var auction = GetAuctionForCreate();
            _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

            // Act: Send a POST request to create the auction.
            var response = await _httpClient.PostAsJsonAsync("api/auctions", auction);

            // Assert: Ensure the request was successful and the AuctionCreated event was published.
            response.EnsureSuccessStatusCode();
            Assert.True(await _testHarness.Published.Any<AuctionCreated>());

        }

    }
}
