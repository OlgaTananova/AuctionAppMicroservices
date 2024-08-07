﻿using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService.Consumers
{
    public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
    {
        public async Task Consume(ConsumeContext<AuctionFinished> context)
        {
            Item auction = await DB.Find<Item>().OneAsync(Guid.Parse(context.Message.AuctionId));
            
            if (context.Message.ItemSold)
            {
                auction.Winner = context.Message.Winner;
                auction.SoldAmount = (int)context.Message.Amount;
            }
            auction.Status = "Finished";
            await auction.SaveAsync();
        }
    }
}
