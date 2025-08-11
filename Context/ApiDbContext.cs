using System.Numerics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MonadNftMarket.Models;

namespace MonadNftMarket.Context;

public class ApiDbContext(DbContextOptions<ApiDbContext> options) : DbContext(options)
{
    public DbSet<Listing> Listings { get; set; }
    public DbSet<Trade> Trades { get; set; }
    public DbSet<IndexerState> Indexer { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var bigIntListConverter 
            = new ValueConverter<List<BigInteger>, string>(
                toDb     => JsonSerializer.Serialize(toDb, (JsonSerializerOptions?)null),
                fromDb   => JsonSerializer.Deserialize<List<BigInteger>>(fromDb, (JsonSerializerOptions?)null) 
                            ?? new List<BigInteger>());
        
        modelBuilder.Entity<IndexerState>()
            .HasData(new IndexerState
            {
                Id = 1,
                LastProcessedBlock = 0
            });
        
        modelBuilder.Entity<IndexerState>()
            .Property(e => e.LastProcessedBlock)
            .HasConversion(
                v => v.ToString(),
                v => BigInteger.Parse(v))
            .HasColumnType("text");
        
        modelBuilder.Entity<Listing>()
            .Property(e => e.ListingId)
            .HasConversion(
                v => v.ToString(),
                v => BigInteger.Parse(v))
            .HasColumnType("text");
        
        modelBuilder.Entity<Listing>()
            .Property(e => e.TokenId)
            .HasConversion(
                v => v.ToString(),
                v => BigInteger.Parse(v))
            .HasColumnType("text");

        modelBuilder.Entity<Listing>()
            .OwnsOne(e => e.EventMetadata, meta =>
            {
                meta.Property(m => m.BlockNumber)
                    .HasConversion(v => v.ToString(),
                        v => BigInteger.Parse(v))
                    .HasColumnType("text");
            });
        
        modelBuilder.Entity<Trade>()
            .OwnsOne(e => e.EventMetadata, meta =>
            {
                meta.Property(m => m.BlockNumber)
                    .HasConversion(v => v.ToString(),
                        v => BigInteger.Parse(v))
                    .HasColumnType("text");
            });
        
        modelBuilder.Entity<Trade>()
            .Property(e => e.TradeId)
            .HasConversion(
                v => v.ToString(),
                v => BigInteger.Parse(v))
            .HasColumnType("text");
        
        modelBuilder.Entity<Trade>()
            .OwnsOne(e => e.From, peer =>
            {
                peer.Property(m => m.TokenIds)
                    .HasConversion(bigIntListConverter)
                    .HasColumnType("text");
            });
        
        modelBuilder.Entity<Trade>()
            .OwnsOne(e => e.To, peer =>
            {
                peer.Property(m => m.TokenIds)
                    .HasConversion(bigIntListConverter)
                    .HasColumnType("text");
            });
        
        modelBuilder.Entity<Listing>()
            .HasIndex(l => l.ListingId)
            .IsUnique();
        
        modelBuilder.Entity<Trade>()
            .HasIndex(l => l.TradeId)
            .IsUnique();
    }
}