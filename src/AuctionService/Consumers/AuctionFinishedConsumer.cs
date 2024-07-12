

using AuctionService.Data;
using AuctionService.Entities;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers
{
    // Method to consume the AuctionFinished message
    public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
    {
        private readonly AuctionDbContext _dbcontext;

        public AuctionFinishedConsumer(AuctionDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        public async Task Consume(ConsumeContext<AuctionFinished> context)
        {
            Console.WriteLine("--> Consuming auction finished");

            // Find the auction in the database using the auction ID from the message
            Auction auction = await _dbcontext.Auctions.FindAsync(context.Message.AuctionId);

            // Check if the item was sold and update the auction details
            if (context.Message.ItemSold)
            {
                auction.Winner = context.Message.Winner;
                auction.SoldAmount = context.Message.Amount;
            }

            // Update the auction status based on whether the sold amount exceeds the reserve price
            auction.Status = auction.SoldAmount > auction.ReservePrice ?
                Status.Finished : Status.ReserveNotMet;

            await _dbcontext.SaveChangesAsync();
        }
    }
}
