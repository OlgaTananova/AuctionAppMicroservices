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

        if (count == 0){

            Console.WriteLine("No data - will attempt to seed");
            string itemData = await File.ReadAllTextAsync("Data/auctions.json");
            JsonSerializerOptions options = new JsonSerializerOptions{PropertyNameCaseInsensitive = true};

            List<Item> items = JsonSerializer.Deserialize<List<Item>>(itemData, options);

            await DB.SaveAsync(items);

        }
    }
}
