using System.Numerics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MonadNftMarket.Models;

namespace MonadNftMarket.Context;

public class ApiDbContext(DbContextOptions<ApiDbContext> options) : DbContext(options)
{
    public DbSet<Listing> Listings { get; set; }
    public DbSet<Trade> Trades { get; set; }
    public DbSet<IndexerState> Indexer { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<History> History { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var bigIntListConverter = new ValueConverter<List<BigInteger>, string>(
            toDb   => JsonSerializer.Serialize(toDb.Select(b => b.ToString()), (JsonSerializerOptions?)null),
            fromDb => JsonSerializer.Deserialize<List<string>>(fromDb, (JsonSerializerOptions?)null)!
                .Select(BigInteger.Parse).ToList()
        );

        var bigIntListComparer = new ValueComparer<List<BigInteger>>(
            (a, b) => a == null && b == null || (a != null && b != null && a.SequenceEqual(b)),
            a => a.Aggregate(0, (h, v) => HashCode.Combine(h, v.GetHashCode())),
            a => a.ToList());
        
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

        modelBuilder.Entity<History>()
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
                    .HasColumnType("text")
                    .Metadata.SetValueComparer(bigIntListComparer);
            });
        
        modelBuilder.Entity<Trade>()
            .OwnsOne(e => e.To, peer =>
            {
                peer.Property(m => m.TokenIds)
                    .HasConversion(bigIntListConverter)
                    .HasColumnType("text")
                    .Metadata.SetValueComparer(bigIntListComparer);
                    
            });

        modelBuilder.Entity<Trade>(entity =>
        {
            entity.Property(e => e.ListingIds)
                .HasConversion(bigIntListConverter)
                .Metadata.SetValueComparer(bigIntListComparer);
        });

        modelBuilder.Entity<Listing>(e =>
        {
            e.HasKey(l => l.Id);
            e.HasAlternateKey(l => l.ListingId);
            e.HasIndex(l => l.ListingId).IsUnique();
        });

        modelBuilder.Entity<Trade>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasAlternateKey(t => t.TradeId);
            e.HasIndex(t => t.TradeId).IsUnique();
        });

        modelBuilder.Entity<History>(e =>
        {
            e.HasKey(h => h.Id);

            e.HasOne(h => h.Listing)
                .WithMany()
                .HasForeignKey(h => h.ListingId)
                .HasPrincipalKey(h => h.ListingId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            e.HasOne(h => h.Trade)
                .WithMany()
                .HasForeignKey(h => h.TradeId)
                .HasPrincipalKey(h => h.TradeId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            e.HasIndex(h => h.ListingId);
            e.HasIndex(h => h.TradeId);
        });

        modelBuilder.Entity<Listing>()
            .Property(l => l.Status)
            .HasConversion<int>();
        
        modelBuilder.Entity<Trade>()
            .Property(l => l.Status)
            .HasConversion<int>();
        
        modelBuilder.Entity<History>()
            .Property(l => l.Status)
            .HasConversion<int>();
        
        modelBuilder.Entity<Notification>()
            .Property(n => n.Status)
            .HasConversion<int>();

        modelBuilder.Entity<Listing>()
            .HasIndex(l => new { l.Status, l.ListingId });
        
        modelBuilder.Entity<History>()
            .HasIndex(h => new { h.FromAddress, h.Status });
        
        modelBuilder.Entity<History>()
            .HasIndex(h => new { h.FromAddress, h.ToAddress, h.Status });

        modelBuilder.Entity<Trade>()
            .HasIndex(l => new { l.Status, l.TradeId });

        modelBuilder.Entity<Listing>()
            .OwnsOne<NftMetadata>(l => l.NftMetadata);
        
        base.OnModelCreating(modelBuilder);
    }
}