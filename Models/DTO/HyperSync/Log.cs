using System.Text.Json.Serialization;

namespace MonadNftMarket.Models.DTO.HyperSync;

public class Log
{
    [JsonPropertyName("log_index")]
    public long LogIndex { get; set; }
    
    [JsonPropertyName("transaction_index")]
    public long TransactionIndex { get; set; }
    
    [JsonPropertyName("block_number")]
    public long BlockNumber { get; set; }
    
    [JsonPropertyName("address")]
    public string? Address { get; set; }
    
    [JsonPropertyName("data")]
    public string? Data { get; set; }
    
    [JsonPropertyName("topic0")]
    public string? Topic0 { get; set; }
    
    [JsonPropertyName("topic1")]
    public string? Topic1 { get; set; }
    
    [JsonPropertyName("topic2")]
    public string? Topic2 { get; set; }
    
    [JsonPropertyName("topic3")]
    public string? Topic3 { get; set; }
}