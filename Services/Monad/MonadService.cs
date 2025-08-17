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
        var monadRpc = env.Value.MonadRpcUrl;
        _web3 = new Web3(monadRpc);
        _contractAddress = env.Value.ContractAddress;
        _logger = logger;
    }

    public async Task<GetTradeOutput> GetTradeDataAsync(BigInteger tradeId, CancellationToken cancellationToken = default)
    {
        var abi = await File.ReadAllTextAsync("Services/Monad/abi.json", cancellationToken);
        var contract = _web3.Eth.GetContract(abi, _contractAddress);
        
        var func = contract.GetFunction<GetTradeFunction>();
        
        var function = contract.GetFunction("trades");
        var callInput = function.CreateCallInput(tradeId);
        _logger.LogCritical("Trade id from monad service: {id}", tradeId);
        var rawHex = await _web3.Eth.Transactions.Call.SendRequestAsync(callInput);
        _logger.LogCritical("Raw result hex: {RawHex}", rawHex);

        var getTradeMsg = new GetTradeFunction { TradeId = tradeId };

        GetTradeOutput trade = new();
        try
        {
            trade = await func.CallDeserializingToObjectAsync<GetTradeOutput>(getTradeMsg);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in deserialization of trade data: {ex.Message}");
        }

        return trade;
    }
}