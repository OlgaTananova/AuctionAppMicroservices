using MassTransit;
using Contracts;
using AutoMapper;
using MongoDB.Entities;

namespace SearchService
{
    public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
    {
        private readonly IMapper _mapper;

        public AuctionCreatedConsumer(IMapper mapper)
        {
            _mapper = mapper;
        }
        public async Task Consume(ConsumeContext<AuctionCreated> context)
        {
            Console.WriteLine("--> Consuming auction created: " + context.Message.Id);

            Item item = _mapper.Map<Item>(context.Message);

            // Model the situation if there is an exception in the consumer --> the exception will be handled by the faulty consumer
            if (item.Model == "Foo") throw new ArgumentException("Cannot sell cars with the name of foo");

            await item.SaveAsync();
        }
    }
}
