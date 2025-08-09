using System.Numerics;

namespace MonadNftMarket.Models;

public class Listing
{
    public Guid Id { get; set; }
    public EventMetadata EventMetadata { get; set; } = new();
    public BigInteger ListingId { get; set; }
    public string? NftContractAddress { get; set; }
    public BigInteger TokenId { get; set; }
    public string? SellerAddress { get; set; }
    public decimal Price { get; set; }
    public bool IsSold { get; set; }
    public bool IsActive { get; set; }
    public string? BuyerAddress { get; set; }
}