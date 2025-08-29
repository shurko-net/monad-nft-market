using System.Numerics;

namespace MonadNftMarket.Models.DTO;

public class ListingResponse
{
    public BigInteger ListingId { get; set; }
    public string? ContractAddress { get; set; }
    public BigInteger TokenId { get; set; }
    public string? SellerAddress { get; set; }
    public decimal Price { get; set; }
    public required Metadata Metadata { get; set; }
    public bool IsOwnedByCurrentUser { get; set; }
    public EventStatus Status { get; set; }
    public DateTime LastUpdated { get; set; }
}