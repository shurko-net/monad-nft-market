using System.Text.Json.Serialization;

namespace MonadNftMarket.Models.DTO.HyperSync;

public class Transaction
{
    [JsonPropertyName("block_number")]
    public long BlockNumber { get; set; }
    
    [JsonPropertyName("from")]
    public string? From { get; set; }
    
    [JsonPropertyName("hash")]
    public string? Hash { get; set; }
    
    [JsonPropertyName("input")]
    public string? Input { get; set; }
    
    [JsonPropertyName("to")]
    public string? To { get; set; }
    
    [JsonPropertyName("transaction_index")]
    public long TransactionIndex { get; set; }
    
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}