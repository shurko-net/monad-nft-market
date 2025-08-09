using System.Numerics;

namespace MonadNftMarket.Models;

public class IndexerState
{
    public int Id { get; set; }
    public BigInteger LastProcessedBlock { get; set; }
    public DateTime UpdatedAt { get; set; }
}