using System.Text.Json.Serialization;

namespace MonadNftMarket.Models.DTO.HyperSync;

public class Root
{
    [JsonPropertyName("data")] public List<Data> Data { get; set; } = new();
    [JsonPropertyName("archive_height")]
    public long? ArchiveHeight { get; set; }
    
    [JsonPropertyName("next_block")]
    public long? NextBlock { get; set; }
    
    [JsonPropertyName("total_execution_time")]
    public long TotalExecutionTime { get; set; }
    
    [JsonPropertyName("rollback_guard")]
    public object? RollbackGuard { get; set; }
}