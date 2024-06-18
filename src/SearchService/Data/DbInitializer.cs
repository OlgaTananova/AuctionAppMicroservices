using System.Text.Json;
using Microsoft.AspNetCore.Http.Features;
using MongoDB.Driver;
using MongoDB.Entities;

namespace SearchService;

public class DbInitializer
{
    public static async Task InitDb(WebApplication app)
    {
        // Initializing a dababase
        await DB.InitAsync("SearchDb", MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

        // creating indexes on Make, Model, Color properties, which allow to perform a search on these fields
         await DB.Index<Item>()
        .Key(x => x.Make, KeyType.Text)
        .Key(x => x.Model, KeyType.Text)
        .Key(x => x.Color, KeyType.Text)
        .CreateAsync();

        var count = await DB.CountAsync<Item>();

        IServiceScope scope = app.Services.CreateScope();
        AuctionServiceHttpClient httpClient = scope.ServiceProvider.GetRequiredService<AuctionServiceHttpClient>();
        List<Item> items = await httpClient.GetItemsForSearchDb();
        Console.WriteLine(items.Count);

        if(items.Count >  0) await DB.SaveAsync(items);
        
    }
}
