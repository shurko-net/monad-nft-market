using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace MonadNftMarket.Models;

public class History
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    [Required, MaxLength(50)]
    public string FromAddress
    {
        get => _fromAddress;
        init => _fromAddress = string.IsNullOrEmpty(value) ? string.Empty : NormalizeAddress(value);
    }
    [Required, MaxLength(50)]
    public string ToAddress
    {
        get => _toAddress;
        set => _toAddress = string.IsNullOrEmpty(value) ? string.Empty : NormalizeAddress(value);
    }
    public EventMetadata EventMetadata { get; init; } = new();
    public BigInteger? ListingId { get; init; }
    public BigInteger? TradeId { get; init; }
    public EventStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    
    public Trade? Trade { get; set; } = new();
    public Listing? Listing { get; set; } = new();
    
    private readonly string _fromAddress = string.Empty;
    private string _toAddress = string.Empty;
    private static string NormalizeAddress(string? addr) =>
        string.IsNullOrWhiteSpace(addr) ? string.Empty : addr.Trim().ToLowerInvariant();
}