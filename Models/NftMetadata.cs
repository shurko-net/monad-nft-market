using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Microsoft.EntityFrameworkCore;

namespace MonadNftMarket.Models;

[Owned]
public class NftMetadata
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public BigInteger TokenId { get; init; }
    [Required, MaxLength(50)]
    public string NftContractAddress
    {
        get => _nftContractAddress;
        init => _nftContractAddress = string.IsNullOrEmpty(value) ? string.Empty : NormalizeAddress(value);
    }
    [MaxLength(50)] public string Kind { get; set; } = string.Empty;
    [MaxLength(50)] public string Name { get; set; } = string.Empty;
    [DataType(DataType.Text)] public string ImageOriginal { get; set; } = string.Empty;
    [DataType(DataType.Text)] public string Description { get; set; } = string.Empty;
    public decimal? LastPrice { get; set; }
    public DateTime LastUpdated { get; set; }
    
    private readonly string _nftContractAddress = string.Empty;
    private static string NormalizeAddress(string? addr) =>
        string.IsNullOrWhiteSpace(addr) ? string.Empty : addr.Trim().ToLowerInvariant();
}
