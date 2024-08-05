using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService;

// Background service to check for finished auctions
public class CheckAuctionFinished : BackgroundService
{
    private readonly ILogger<CheckAuctionFinished> _logger;
    private readonly IServiceProvider _services;

    public CheckAuctionFinished(ILogger<CheckAuctionFinished> logger, IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting check for finished auctions");

        // Register a cancellation callback to log when the auction check is stopping

        stoppingToken.Register(() => _logger.LogInformation("==> Auction check is stopping"));

        // Loop to continuously check for finished auctions until the service is cancelled
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckAuctions(stoppingToken);
            await Task.Delay(5000, stoppingToken);
        }
    }

    private async Task CheckAuctions(CancellationToken stoppingToken)
    {
        // Query to find auctions that have finished but are not marked as finished
        var finishedAuctions = await DB.Find<Auction>()
        .Match(x => x.AuctionEnd <= DateTime.UtcNow)
        .Match(x => !x.Finished)
        .ExecuteAsync(stoppingToken);

        if (finishedAuctions.Count == 0) return;

        _logger.LogInformation($"==> Found {finishedAuctions.Count} auctions that have completed.");

        using var scope = _services.CreateScope();
        var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        // Iterate over each finished auction
        foreach (var auction in finishedAuctions)
        {
            // Mark the auction as finished
            auction.Finished = true;
            await auction.SaveAsync(null, stoppingToken);

            // Find the highest accepted bid for the auction
            var winningBid = await DB.Find<Bid>()
            .Match(x => x.AuctionId == auction.ID)
            .Match(x => x.BidStatus == BidStatus.Accepted)
            .Sort(x => x.Descending(s => s.Amount))
            .ExecuteFirstAsync(stoppingToken);

            // Publish an AuctionFinished event with the details of the auction and winning bid
            await endpoint.Publish(new AuctionFinished
            {
                ItemSold = winningBid != null,
                AuctionId = auction.ID,
                Winner = winningBid?.Bidder,
                Amount = winningBid?.Amount,
                Seller = auction.Seller,
            }, stoppingToken);
        }

    }
}
