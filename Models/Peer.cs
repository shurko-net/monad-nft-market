using Microsoft.EntityFrameworkCore;

namespace MonadNftMarket.Models;

[Owned]
public class Peer
{
    public string? Address { get; set; }
    public List<string> TokenIds { get; set; } = [];
    public List<string> NftContracts { get; set; } = [];
}