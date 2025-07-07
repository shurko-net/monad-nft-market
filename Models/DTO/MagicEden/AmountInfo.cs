using System.Text.Json.Serialization;

namespace MonadNftMarket.Models.MagicEden.DTO;

public class AmountInfo
{
    [JsonPropertyName("decimal")]
    public decimal Decimal { get; set; }
    [JsonPropertyName("native")]
    public decimal Native { get; set; }
}