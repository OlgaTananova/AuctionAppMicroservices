using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Driver;
using MongoDB.Entities;

namespace SearchService
{
    public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
    {

        public async Task Consume(ConsumeContext<AuctionDeleted> context)
        {
            Console.WriteLine("--> Consuming auction deleted: " + context.Message.Id);


            DeleteResult result = await DB.DeleteAsync<Item>(context.Message.Id);

            if (!result.IsAcknowledged)
                throw new MessageException(typeof(AuctionUpdated), "Problem deleting an item in mongodb");

        }
    }
}
