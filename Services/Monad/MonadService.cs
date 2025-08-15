using System.Numerics;
using Microsoft.Extensions.Options;
using MonadNftMarket.Configuration;
using MonadNftMarket.Models.ContractFunctions;
using MonadNftMarket.Models.ContractOutput;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.Web3;

namespace MonadNftMarket.Services.Monad;

public class MonadService : IMonadService
{
    private readonly Web3 _web3;
    private readonly string _contractAddress;
    private readonly string _monadRpc;
    private readonly ILogger<MonadService> _logger;
    private readonly HttpClient _http;
    public MonadService(IOptions<EnvVariables> env,
        ILogger<MonadService> logger)
    {
        _monadRpc = env.Value.MonadRpcUrl;
        _web3 = new Web3(_monadRpc);
        _contractAddress = env.Value.ContractAddress;
        _logger = logger;
        _http = new HttpClient();
    }

    public async Task<GetTradeOutput> GetTradeDataAsync(BigInteger tradeId, CancellationToken cancellationToken = default)
    {
        var abi = await File.ReadAllTextAsync("Services/Monad/abi.json", cancellationToken);
        var contract = _web3.Eth.GetContract(abi, _contractAddress);

        var func = contract.GetFunction<GetTradeFunction>();

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

    private T DecodeRawHexToDto<T>(string rawHex) where T : new()
    {
        var decoder = new FunctionCallDecoder();
        
        T dto = decoder.DecodeFunctionOutput<T>(rawHex);
        
        return dto;
    }
}