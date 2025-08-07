using System.Text.Json.Serialization;

namespace MonadNftMarket.Models.DTO.HyperSync;

public class Block
{
    [JsonPropertyName("number")]
    public long Number { get; set; }
    
    [JsonPropertyName("hash")]
    public string? Hash { get; set; }
    
    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }
}