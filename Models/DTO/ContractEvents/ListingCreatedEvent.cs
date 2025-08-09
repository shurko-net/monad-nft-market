using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace MonadNftMarket.Models.DTO.ContractEvents;

[Event("ListingCreated")]
public class ListingCreatedEvent : IEventDTO
{
    [Parameter("uint256", "id", 1, true)]
    public BigInteger Id { get; set; }
    [Parameter("address", "nftContract", 2, true)]
    public string? NftContract { get; set; }
    [Parameter("uint256", "tokenId", 3, true)]
    public BigInteger TokenId { get; set; }
    [Parameter("address", "seller",4, false)] 
    public string? Seller { get; set; }
    [Parameter("uint256", "price", 5, false)]
    public BigInteger Price { get; set; }
}