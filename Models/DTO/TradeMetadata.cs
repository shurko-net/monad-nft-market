using System.Numerics;

namespace MonadNftMarket.Models.DTO;

public class TradeMetadata
{
    public required BigInteger TradeId { get; set; }
    public required string FromAddress { get; set; }
    public required List<UserToken> From { get; set; }
    public required string ToAddress { get; set; }
    public required List<UserToken> To { get; set; }
}