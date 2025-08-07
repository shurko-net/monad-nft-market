using Microsoft.EntityFrameworkCore;
using MonadNftMarket.Models;

namespace MonadNftMarket.Context;

public class ApiDbContext(DbContextOptions<ApiDbContext> options) : DbContext(options)
{
    public DbSet<Listing> Listings { get; set; }
    public DbSet<Trade> Trades { get; set; }
    public DbSet<IndexerState> Indexer { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IndexerState>()
            .HasData(new IndexerState
            {
                Id = 1,
                LastProcessedBlock = "0"
            });
    }
}