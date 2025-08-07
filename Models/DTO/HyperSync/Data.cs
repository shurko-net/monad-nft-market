using System.Text.Json;
using System.Text.Json.Serialization;

namespace MonadNftMarket.Models.DTO.HyperSync;

public class Data
{
    [JsonPropertyName("logs")]
    public List<Log> Logs { get; set; } = [];
    
    [JsonPropertyName("transactions")]
    public List<Transaction> Transactions { get; set; } = [];
    
    [JsonPropertyName("blocks")]
    public List<Block> Blocks { get; set; } = [];
    
    [JsonPropertyName("archive_height")]
    public long? ArchiveHeight { get; set; }
    
    [JsonPropertyName("next_block")]
    public long? NextBlock { get; set; }
    
    [JsonPropertyName("total_execution_time")]
    public long TotalExecutionTime { get; set; }
    
    [JsonPropertyName("rollback_guard")]
    public JsonElement? RollbackGuard { get; set; }
}