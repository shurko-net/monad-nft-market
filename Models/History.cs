using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace MonadNftMarket.Models;

public class History
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    [Required, MaxLength(50)]
    public string UserAddress
    {
        get => _userAddress;
        init => _userAddress = string.IsNullOrEmpty(value) ? string.Empty : NormalizeAddress(value);
    }
    public EventMetadata EventMetadata { get; init; } = new();
    public BigInteger? ListingId { get; init; }
    public BigInteger? TradeId { get; init; }
    public EventStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    
    private readonly string _userAddress = string.Empty;
    private static string NormalizeAddress(string? addr) =>
        string.IsNullOrWhiteSpace(addr) ? string.Empty : addr.Trim().ToLowerInvariant();
}