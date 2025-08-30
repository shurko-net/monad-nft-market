using System.Numerics;

namespace MonadNftMarket.Models.DTO;

public class HistoryDto
{
    public required string UserAddress { get; set; }
    public required HistoryStatus Status { get; set; }
    public required EventMetadata Metadata { get; set; }
    public BigInteger? ListingId { get; set; }
    public BigInteger? TradeId { get; set; }
    public Trade? Trade { get; set; }
    public Listing? Listings { get; set; }
}