using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class AuctionDbContext : DbContext
{
    public AuctionDbContext(DbContextOptions options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder){

        modelBuilder.Entity<Auction>()
        .HasOne(e => e.Item);
        
        modelBuilder.Entity<Item>()
        .HasOne(e => e.Auction);
    }
    

    public DbSet<Auction> Auctions { get; set; }
}
