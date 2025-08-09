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
}