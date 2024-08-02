using AuctionService.Data;
using AuctionService.Entities;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers
{
    // Consumer class for handling BidPlaced events
    public class BidPlacedConsumer : IConsumer<BidPlaced>
    {
        private readonly AuctionDbContext _dbcontext;

        public BidPlacedConsumer(AuctionDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        public async Task Consume(ConsumeContext<BidPlaced> context)
        {
            Console.WriteLine("--> Consuming bid placed");

            Auction auction = await _dbcontext.Auctions.FindAsync(Guid.Parse(context.Message.AuctionId));

            // Check if there is no current high bid or if the new bid is accepted and higher than the current high bid

            if (auction.CurrentHighBid  == null || context.Message.BidStatus.Contains("Accepted") && context.Message.Amount > auction.CurrentHighBid)
            {
                // Update the auction's current high bid with the new bid amount
                auction.CurrentHighBid = context.Message.Amount;

                await _dbcontext.SaveChangesAsync();
            }

        }
    }
}
