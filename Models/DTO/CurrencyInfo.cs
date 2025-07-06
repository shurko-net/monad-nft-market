using System.Text.Json.Serialization;

namespace MonadNftMarket.Models.DTO;

public class CurrencyInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;
    [JsonPropertyName("decimals")]
    public int Decimals { get; set; }
}