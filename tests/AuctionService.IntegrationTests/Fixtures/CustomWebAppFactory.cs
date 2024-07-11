using AuctionService.Data;
using AuctionService.IntegrationTests.Util;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using WebMotions.Fake.Authentication.JwtBearer;

namespace AuctionService.IntegrationTests.Fixtures
{
    // CustomWebAppFactory class is used for setting up an integration testing environment
    // for the AuctionService application. It extends WebApplicationFactory to customize the 
    // web host configuration and implements IAsyncLifetime for async setup and teardown.

    public class CustomWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        // PostgreSqlContainer instance for managing a PostgreSQL database in a test container.

        private PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

        // InitializeAsync method to start the PostgreSQL container asynchronously.

        public async Task InitializeAsync()
        {
            await _postgreSqlContainer.StartAsync();
        }

        // ConfigureWebHost method to configure the web host for integration tests.

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                // Remove the existing db context

                services.RemoveDbContext<AuctionDbContext>();

                // Add a new AuctionDbContext configuration to use the PostgreSQL test container.

                services.AddDbContext<AuctionDbContext>(options =>
                {
                    options.UseNpgsql(_postgreSqlContainer.GetConnectionString());
                });

                // Add MassTransit test harness for testing message-based interactions.

                services.AddMassTransitTestHarness();

                // Ensure that the db is created and seeded

                services.EnsureCreated<AuctionDbContext>();

                // Add Jwt token to test the Authentication

                services.AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme)
                .AddFakeJwtBearer(opt =>
                {
                    opt.BearerValueType = FakeJwtBearerBearerValueType.Jwt;
                });
            });
        }

        // DisposeAsync method to dispose of the PostgreSQL container asynchronously.

        Task IAsyncLifetime.DisposeAsync() => _postgreSqlContainer.DisposeAsync().AsTask();
    }
}
