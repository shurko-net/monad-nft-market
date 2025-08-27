using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Microsoft.EntityFrameworkCore;

namespace MonadNftMarket.Models;

[Owned]
public class EventMetadata
{
    public BigInteger BlockNumber { get; init; }
    [MaxLength(70)] public string BlockHash { get; set; } = string.Empty;
    public DateTime Timestamp { get; init; }
    [MaxLength(70)] public string TransactionHash { get; init; } = string.Empty;
}