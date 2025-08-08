using System.Text.Json.Serialization;

namespace MonadNftMarket.Models.DTO.HyperSync;

public class HyperSyncWrapper
{
    [JsonPropertyName("data")] public Data Data { get; set; } = new();
}