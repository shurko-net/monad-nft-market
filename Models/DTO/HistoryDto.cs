using System.Numerics;
using System.Text.Json.Serialization;

namespace MonadNftMarket.Models.DTO;

public class HistoryDto
{
    public required string UserAddress { get; set; }
    public required EventStatus Status { get; set; }
    public required EventMetadata Metadata { get; set; }
    public BigInteger? ListingId { get; set; }
    public BigInteger? TradeId { get; set; }
}