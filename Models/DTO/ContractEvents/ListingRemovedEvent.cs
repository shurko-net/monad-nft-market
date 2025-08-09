using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace MonadNftMarket.Models.DTO.ContractEvents;

[Event("ListingRemoved")]
public class ListingRemovedEvent : IEventDTO
{
    [Parameter("uint256", "id", 1, true)]
    public BigInteger Id { get; set; }
    
    [Parameter("address", "owner", 2, true)]
    public string? Owner { get; set; }
}