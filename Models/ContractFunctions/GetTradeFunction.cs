using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace MonadNftMarket.Models.ContractFunctions;

[Function("getTrade")]
public class GetTradeFunction : FunctionMessage
{
    [Parameter("uint256", "tradeId", 1)]
    public BigInteger TradeId { get; set; }
}