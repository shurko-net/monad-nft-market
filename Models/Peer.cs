using System.Numerics;
using Microsoft.EntityFrameworkCore;

namespace MonadNftMarket.Models;

[Owned]
public class Peer
{
    public string? Address { get; set; }
    public List<BigInteger> TokenIds { get; set; } = new();
    public List<string> NftContracts { get; set; } = new();
}