using System.Numerics;
using Microsoft.EntityFrameworkCore;

namespace MonadNftMarket.Models;

[Owned]
public class EventMetadata
{
    public BigInteger BlockNumber { get; set; }
    public string? BlockHash { get; set; }
    public DateTime Timestamp { get; set; }
    public string? TransactionHash { get; set; }
}