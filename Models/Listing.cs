using System.Numerics;

namespace MonadNftMarket.Models;

public class Listing
{
    private string? _nftContractAddress;
    private string? _sellerAddress;
    private string? _buyerAddress;
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public EventMetadata EventMetadata { get; set; } = new();
    public BigInteger ListingId { get; set; }
    public string? NftContractAddress
    {
        get => _nftContractAddress;
        set => _nftContractAddress = value?.Trim().ToLowerInvariant();
    }
    public BigInteger TokenId { get; set; }
    public string? SellerAddress
    {
        get => _sellerAddress;
        set => _sellerAddress = value?.Trim().ToLowerInvariant();
    }
    public decimal Price { get; set; }
    public bool IsSold { get; set; }
    public bool IsActive { get; set; }
    public string? BuyerAddress
    {
        get => _buyerAddress;
        set => _buyerAddress = value?.Trim().ToLowerInvariant();
    }
}