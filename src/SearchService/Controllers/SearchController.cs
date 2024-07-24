using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using ZstdSharp.Unsafe;

namespace SearchService;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
    {
        // Get all items from the db
        var query = DB.PagedSearch<Item, Item>();


        // if the search param is not empty - do the search using indexes
        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
        }

        // Sort the query
        query = searchParams.OrderBy switch
        {
            "make" => query.Sort(x => x.Ascending(a => a.Make))
                            .Sort(x => x.Ascending(a => a.Model)),
            "new" => query.Sort(x => x.Descending(a => a.CreatedAt)),
            _ => query.Sort(x => x.Ascending(a => a.AuctionEnd)),
        };

            query = searchParams.FilerBy switch
            {
                "finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
                "endingSoon" => query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6) && x.AuctionEnd > DateTime.UtcNow),
                _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
            };


        if (!string.IsNullOrEmpty(searchParams.Seller))
        {

            query.Match(x => x.Seller == searchParams.Seller);
        }

        if (!string.IsNullOrEmpty(searchParams.Winner))
        {

            query.Match(x => x.Winner == searchParams.Winner);
        }

        // Do pagination
        query.PageNumber(searchParams.PageNumber);
        query.PageSize(searchParams.PageSize);

        var result = await query.ExecuteAsync();
        Console.WriteLine(result.Results);
        return Ok(new
        {
            result = result.Results,
            pageCount = result.PageCount,
            totalCount = result.TotalCount,
        });
    }
}
