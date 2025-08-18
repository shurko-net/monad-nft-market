using System.Numerics;
using Microsoft.EntityFrameworkCore;

namespace MonadNftMarket.Models;

[Owned]
public class Peer
{
    private string? _address;
    public string? Address
    {
        get => _address;
        set => _address = value?.Trim().ToLowerInvariant();
    }
    public List<BigInteger> TokenIds { get; set; } = new();
    public List<string> NftContracts { get; set; } = new();
}