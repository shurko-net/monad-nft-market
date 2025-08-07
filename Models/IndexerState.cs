namespace MonadNftMarket.Models;

public class IndexerState
{
    public int Id { get; set; }
    public string? LastProcessedBlock { get; set; }
    public DateTime UpdatedAt { get; set; }
}