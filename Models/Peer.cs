using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Microsoft.EntityFrameworkCore;

namespace MonadNftMarket.Models;

[Owned]
public class Peer
{
    [Required, MaxLength(50)]
    public string Address
    {
        get => _address;
        init => _address = string.IsNullOrEmpty(value) ? string.Empty : NormalizeAddress(value);
    }
    public List<BigInteger> TokenIds { get; init; } = new();
    public List<string> NftContracts { get; init; } = new();
    
    private readonly string _address = string.Empty;
    private static string NormalizeAddress(string? addr) =>
        string.IsNullOrWhiteSpace(addr) ? string.Empty : addr.Trim().ToLowerInvariant();
}