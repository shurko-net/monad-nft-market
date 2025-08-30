using System.Numerics;

namespace MonadNftMarket.Models;
public class Trade
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public BigInteger TradeId { get; init; }
    public List<BigInteger> ListingIds { get; init; } = new();
    public Peer From { get; init; } = new();
    public Peer To { get; init; } = new();
    public EventStatus Status { get; set; } 
    
    public IEnumerable<Listing> Listings { get; init; } = [];
}