using System.Numerics;

namespace MonadNftMarket.Models;

public class Trade
{
    public Guid Id { get; set; }
    public EventMetadata EventMetadata { get; set; } = new();
    public BigInteger TradeId { get; set; }
    public Peer From { get; set; } = new();
    public Peer To { get; set; } = new();
    
    public bool IsActive { get; set; }
}