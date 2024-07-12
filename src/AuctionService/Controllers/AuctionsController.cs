using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]

public class AuctionsController: ControllerBase
{
    private readonly IAuctionRepositiory _repo;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    // Constructor to inject dependencies: repository, mapper, and publish endpoint
    public AuctionsController(IAuctionRepositiory repo, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _repo = repo;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    // Endpoint to get all auctions, optionally filtered by date

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date){
        
        return await _repo.GetAuctionsAsync(date);
    }

    // Endpoint to get an auction by its ID

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id){
        
        var auction = await _repo.GetAuctionByIdAsync(id);

        if (auction == null){
            return NotFound();
        }
        return auction;
    }

    // Endpoint to create a new auction, requires authorization

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        // Map the DTO to the Auction entity
        Auction auction = _mapper.Map<Auction>(auctionDto);

        // Set the seller to the current user
        auction.Seller = User.Identity.Name;

        // Add the auction to the repository
        _repo.AddAuction(auction);

        // Map the auction to AuctionDto and publish an AuctionCreated event
        AuctionDto newAuction = _mapper.Map<AuctionDto>(auction);
        await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

        // Save changes to the database
        bool result = await _repo.SaveChangesAsync();

        if (!result) return BadRequest("Could not save changes to the DB");

        // return 201 response and pass the method that returns the created auction and maps it to AuctionDto
        return CreatedAtAction(nameof(GetAuctionById), new {auction.Id}, newAuction);
    }

    // Endpoint to update an existing auction by its ID

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        Auction auction = await _repo.GetAuctionEntityByIdAsync(id);
        
        if (auction == null) return NotFound();


        if (auction.Seller != User.Identity.Name) return Forbid();

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Milage = updateAuctionDto.Milage ?? auction.Item.Milage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        // Create UpdatedAuctionDTO from the auction using Automapper

        AuctionUpdated updatedAuction = _mapper.Map<AuctionUpdated>(auction);

        // Publish an updated auction message to the message brocker

        await _publishEndpoint.Publish(updatedAuction);

        bool result = await _repo.SaveChangesAsync();

        if (!result) return BadRequest("Problem saving changes");

        return Ok("The auction has been updated successfully");
    }

    // Endpoint to delete an auction by its ID

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {

        Auction auction = await _repo.GetAuctionEntityByIdAsync(id);

        if (auction == null) return NotFound();

        if (auction.Seller != User.Identity.Name) return Forbid();

        _repo.RemoveAuction(auction);

        // Publish an event to the massage bus 

        await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });

        bool result = await _repo.SaveChangesAsync();
        
        if (!result) return BadRequest("Could not update DB");

        return Ok();
    }
}
