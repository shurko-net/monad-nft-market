using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace MonadNftMarket.Models.ContractOutput;

[FunctionOutput]
public class PeerOutput : IFunctionOutputDTO
{
    [Parameter("address", "user", 1)]
    public string User { get; set; } = string.Empty;
    [Parameter("uint256[]", "tokenIds", 2)]
    public List<BigInteger> TokenIds { get; set; } = new();
    [Parameter("address[]", "nftContracts", 3)]
    public List<string> NftContracts { get; set; } = new();
}