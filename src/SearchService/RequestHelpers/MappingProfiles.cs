using AutoMapper;
using Contracts;

namespace SearchService
{
    public class MappingProfiles: Profile
    {
        public MappingProfiles()
        {
            // Map entities for Automapper
            CreateMap<AuctionCreated, Item>();
            CreateMap<AuctionUpdated, Item>();
        }
    }
}
