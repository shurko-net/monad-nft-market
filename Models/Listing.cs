namespace MonadNftMarket.Models;

public class Listing
{
    public Guid Id { get; set; }
    public EventMetadata EventMetadata { get; set; } = new();
    
    public long ListingId { get; set; }
    public string? NftContractAddress { get; set; }
    public string? TokenId { get; set; }
    public string? SellerAddress { get; set; }
    public decimal Price { get; set; }
    public bool IsSold { get; set; }
    public bool IsActive { get; set; }
    public string? BuyerAddress { get; set; }
}