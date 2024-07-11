using AuctionService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuctionService.IntegrationTests.Util
{
    // Extension methods for the builder services
    public static class ServiceCollectionExtentions
    {
        public static void RemoveDbContext<T>(this IServiceCollection services)
        {
            // Remove the existing AuctionDbContext configuration if it exists.

            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AuctionDbContext>));
            if (descriptor != null) services.Remove(descriptor);
        }

        public static void EnsureCreated<T>(this IServiceCollection services)
        {
            // Build the service provider, create a scope, and migrate the database.

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AuctionDbContext>();
            db.Database.Migrate();
            DbHelper.InitDbForTests(db);
        }
    }
}
