using Nethereum.ABI.FunctionEncoding.Attributes;

namespace MonadNftMarket.Models.DTO.ContractEvents;
 
[Event("TradeCompleted")]
public class TradeCompletedEvent : IEventDTO
{
    [Parameter("uint256", "tradeId", 1, false)]
    public long TradeId { get; set; }
}