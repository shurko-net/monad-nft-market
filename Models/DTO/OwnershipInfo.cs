using System.Text.Json.Serialization;

namespace MonadNftMarket.Models.DTO;

public class OwnershipInfo
{
    [JsonPropertyName("tokenCount")]
    public string TokenCount { get; set; } = string.Empty;

    [JsonPropertyName("onSaleCount")]
    public string OnSaleCount { get; set; } = string.Empty;

    [JsonPropertyName("acquiredAt")]
    public DateTime AcquiredAt { get; set; }
}