using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]

public class AuctionsController: ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public AuctionsController(AuctionDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(){
        var auctions = await _context.Auctions
        .Include(x => x.Item)
        .OrderBy(x => x.Item.Make)
        .ToListAsync();

        return _mapper.Map<List<AuctionDto>>(auctions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctioinById(Guid id){
        var auction = await _context.Auctions
        .Include(x => x.Item)
        .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null){
            return NotFound();
        }
        return _mapper.Map<AuctionDto>(auction);
    }
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        Auction auction = _mapper.Map<Auction>(auctionDto);
        // TODO: add current user as seller
        auction.Seller = "test";
        _context.Auctions.Add(auction);
        bool result = await _context.SaveChangesAsync() > 0;
        if (!result) return BadRequest("Could not save changes to the DB");

        // return 201 response and pass the method that returns the created auction and maps it to AuctionDto
        return CreatedAtAction(nameof(GetAuctioinById), new {auction.Id}, _mapper.Map<AuctionDto>(auction));
    }

    [HttpPut("{id}")]

    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        Auction auction = await _context.Auctions.Include(i => i.Item)
        .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null) return NotFound();

        // TODO: check selle == username

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Milage = updateAuctionDto.Milage ?? auction.Item.Milage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        bool result = await _context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Problem saving changes");

        return Ok("The auction has been updated successfully");
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {

        Auction auction = await _context.Auctions.FindAsync(id);

        if (auction == null) return NotFound();

        // TODO: check seller = username

        _context.Remove(auction);

        bool result = await _context.SaveChangesAsync() > 0;
        if (!result) return BadRequest("Could not update DB");

        return Ok();
    }
}
