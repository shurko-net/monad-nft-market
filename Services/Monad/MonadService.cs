using System.Numerics;
using Microsoft.Extensions.Options;
using MonadNftMarket.Configuration;
using MonadNftMarket.Models.ContractFunctions;
using MonadNftMarket.Models.ContractOutput;
using Nethereum.Web3;

namespace MonadNftMarket.Services.Monad;

public class MonadService : IMonadService
{
    private readonly Web3 _web3;
    private readonly string _contractAddress;
    private readonly ILogger<MonadService> _logger;

    public MonadService(IOptions<EnvVariables> env,
        ILogger<MonadService> logger)
    {
        _web3 = new Web3(env.Value.MonadRpcUrl);
        _contractAddress = env.Value.ContractAddress;
        _logger = logger;
    }
    
    public async Task<GetTradeOutput> GetTradeData(BigInteger tradeId, CancellationToken cancellationToken = default)
    {
        var contractHandler = _web3.Eth.GetContractQueryHandler<GetTradeFunction>();
        
        var getTradeMsg = new GetTradeFunction { TradeId = tradeId };
        var result = await contractHandler
            .QueryDeserializingToObjectAsync<GetTradeOutput>(getTradeMsg, _contractAddress);
        
        _logger.LogInformation($"Get trade data for {tradeId}");
        
        return result;
    }
}