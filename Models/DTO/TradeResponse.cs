using System.Numerics;

namespace MonadNftMarket.Models.DTO;

public class TradeResponse
{
    public required BigInteger TradeId { get; set; }
    public required string FromAddress { get; set; }
    public required string ToAddress { get; set; }
    public required IEnumerable<TradeMetadataDto> FromMetadata { get; set; }
    public required IEnumerable<TradeMetadataDto> ToMetadata { get; set; }
    public required bool IsIncoming { get; set; }
    public required EventStatus Status { get; set; }
}