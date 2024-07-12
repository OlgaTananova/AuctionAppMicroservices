using Contracts;
using MassTransit;

namespace AuctionService.Consumers
{
    // Consumer class for handling faults during the AuctionCreated event
    public class AuctionCreatedFaultConsumer : IConsumer<Fault<AuctionCreated>>
    {
        // Method to consume the fault message
        public async Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
        {
            Console.WriteLine("--> Consuming faulty creation");

            ExceptionInfo exception = context.Message.Exceptions.First();

            if (exception.ExceptionType == "System.ArgumentException")
            {
                context.Message.Message.Model = "FooBar";
                await context.Publish(context.Message.Message);
            } else
            {
                Console.WriteLine("--> Not an argument exception - update error dashboard somewhere");
            }


        }
    }
}
