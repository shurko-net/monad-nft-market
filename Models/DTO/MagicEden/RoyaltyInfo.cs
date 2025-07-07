using System.Text.Json.Serialization;

namespace MonadNftMarket.Models.MagicEden.DTO;

public class RoyaltyInfo
{
    [JsonPropertyName("bps")]
    public int Bps { get; set; }
    [JsonPropertyName("recipient")]
    public string Recipient { get; set; } = string.Empty;
}