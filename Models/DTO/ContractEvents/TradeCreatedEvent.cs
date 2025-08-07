using Nethereum.ABI.FunctionEncoding.Attributes;

namespace MonadNftMarket.Models.DTO.ContractEvents;

[Event("TradeCreated")]
public class TradeCreatedEvent : IEventDTO
{
    [Parameter("uint256", "tradeId", 1, false)]
    public long TradeId { get; set; }
    
    [Parameter("address", "from", 2, true)]
    public string? From { get; set; }
    
    [Parameter("address", "to", 2, true)]
    public string? To { get; set; }
}