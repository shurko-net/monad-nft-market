using Microsoft.EntityFrameworkCore;

namespace MonadNftMarket.Models;

[Owned]
public class EventMetadata
{
    public long BlockNumber { get; set; }
    public string? BlockHash { get; set; }
    public DateTime Timestamp { get; set; }
    public string? TransactionHash { get; set; }
}