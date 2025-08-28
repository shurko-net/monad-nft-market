using System.Numerics;

namespace MonadNftMarket.Models;

public class Trade
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public EventMetadata EventMetadata { get; init; } = new();
    public BigInteger TradeId { get; init; }
    public Peer From { get; init; } = new();
    public Peer To { get; init; } = new();
    public EventStatus Status { get; set; } 
    public bool IsActive { get; set; }
}