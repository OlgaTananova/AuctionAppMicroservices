using MongoDB.Entities;

namespace SearchService;

public class AuctionServiceHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public AuctionServiceHttpClient(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    // Sends request to AuctionService to get the data about the auctions
    public async Task<List<Item>> GetItemsForSearchDb(){

        // get the date of the last updated auction
        var lastUpdated = await DB.Find<Item, string>()
        .Sort(x=> x.Descending(y=> y.UpdatedAt))
        .Project(x => x.UpdatedAt.ToString())
        .ExecuteFirstAsync();

        // request the auctions added after the last updated date
        return await _httpClient.GetFromJsonAsync<List<Item>>(_config["AuctionServiceUrl"] + "/api/auctions?date=" + lastUpdated); 
    }
}
