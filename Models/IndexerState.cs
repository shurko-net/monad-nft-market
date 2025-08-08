namespace MonadNftMarket.Models;

public class IndexerState
{
    public int Id { get; set; }
    public long LastProcessedBlock { get; set; }
    public DateTime UpdatedAt { get; set; }
}