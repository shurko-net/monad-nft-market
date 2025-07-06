using System.Text.Json.Serialization;

namespace MonadNftMarket.Models.DTO;

public class FloorAskPriceInfo
{
    [JsonPropertyName("currency")]
    public CurrencyInfo Currency { get; set; } = null!;

    [JsonPropertyName("amount")]
    public AmountInfo Amount { get; set; } = null!;
}