using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Driver;
using MongoDB.Driver.Core.Operations;
using MongoDB.Entities;

namespace SearchService
{
    public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
    {
        private readonly IMapper _mapper;

        public AuctionUpdatedConsumer(IMapper mapper)
        {
            _mapper = mapper;
        }
        public async Task Consume(ConsumeContext<AuctionUpdated> context)
        {
            Console.WriteLine("--> Consuming auction updated: " + context.Message.Id);

            Console.WriteLine("-->" + context.Message.Model);

            Item item = _mapper.Map<Item>(context.Message);

            UpdateResult result= await DB.Update<Item>()
                .Match(i => i.ID == context.Message.Id)
                .Modify(x => x.Make, item.Make)
                .Modify(x => x.Model, item.Model)
                .Modify(x => x.Year, item.Year)
                .Modify(x => x.Color, item.Color)
                .Modify(x => x.Milage, item.Milage)
                .ExecuteAsync();

            if (!result.IsAcknowledged)
                throw new MessageException(typeof(AuctionUpdated), "Problem updating mongodb");

        }
    }
}
