using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;

namespace AuctionService.RequestHelpers;

// A class to map entities classes to DTOs and vise a versa
public class MappingProfiles: Profile 
{
    public MappingProfiles()
    {
        // Map Auction to AuctionDto and include properties from the related Item entity
        CreateMap<Auction, AuctionDto>().IncludeMembers(x => x.Item);

        // Map Item to AuctionDto
        CreateMap<Item, AuctionDto>();

        // Map CreateAuctionDto to Auction and map its properties to the related Item entity
        CreateMap<CreateAuctionDto, Auction>()
            .ForMember(d => d.Item, o => o.MapFrom(s => s));

        // Similar mapping

        CreateMap<CreateAuctionDto, Item>();
        CreateMap<AuctionDto, AuctionCreated>();
        CreateMap<Auction, AuctionUpdated>().IncludeMembers(a => a.Item);
        CreateMap<Item, AuctionUpdated>();

    }
}
