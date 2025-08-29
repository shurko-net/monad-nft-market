using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace MonadNftMarket.Models;

public class Listing
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public NftMetadata NftMetadata { get; init; } = new();
    public BigInteger ListingId { get; init; }
    [Required, MaxLength(50)]
    public string NftContractAddress
    {
        get => _nftContractAddress;
        init => _nftContractAddress = string.IsNullOrEmpty(value) ? string.Empty : NormalizeAddress(value);
    }
    public BigInteger TokenId { get; init; }
    [Required, MaxLength(50)]
    public string SellerAddress
    {
        get => _sellerAddress;
        init => _sellerAddress = string.IsNullOrEmpty(value) ? string.Empty : NormalizeAddress(value);
    }
    public decimal Price { get; init; }
    [MaxLength(50)]
    public string? BuyerAddress
    {
        get => _buyerAddress;
        set => _buyerAddress = string.IsNullOrEmpty(value) ? string.Empty : NormalizeAddress(value);
    }
    public EventStatus Status { get; set; }
    
    private readonly string _nftContractAddress = string.Empty;
    private readonly string _sellerAddress = string.Empty;
    private string _buyerAddress = string.Empty;
    private static string NormalizeAddress(string? addr) =>
        string.IsNullOrWhiteSpace(addr) ? string.Empty : addr.Trim().ToLowerInvariant();
}