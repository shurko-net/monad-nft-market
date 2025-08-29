using System.Numerics;

namespace MonadNftMarket.Models;

public class History
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public EventMetadata EventMetadata { get; init; } = new();
    public BigInteger? ListingId { get; init; }
    public BigInteger? TradeId { get; init; }
    public EventStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
}