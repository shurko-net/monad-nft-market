using Nethereum.ABI.FunctionEncoding.Attributes;

namespace MonadNftMarket.Models.DTO.ContractEvents;

[Event("ListingSold")]
public class ListingSoldEvent : IEventDTO
{ 
    [Parameter("uint256", "id", 1, true)]
    public long Id { get; set; }
    
    [Parameter("address", "owner", 2, true)]
    public string? Buyer { get; set; }
}