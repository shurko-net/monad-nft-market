using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace MonadNftMarket.Models.ContractEvents;

[Event("TradeAccepted")]
public class TradeAcceptedEvent : IEventDTO
{
    [Parameter("uint256", "tradeId", 1, false)]
    public BigInteger TradeId { get; set; }
}