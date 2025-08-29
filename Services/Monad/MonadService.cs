using System.Numerics;
using Microsoft.Extensions.Options;
using MonadNftMarket.Configuration;
using MonadNftMarket.Models.ContractFunctions;
using MonadNftMarket.Models.ContractOutput;
using Nethereum.Web3;
using Polly;

namespace MonadNftMarket.Services.Monad;

public class MonadService : IMonadService
{
    private static readonly Random Jitterer = new();
    private readonly Web3 _web3;
    private readonly string _contractAddress;
    private readonly ILogger<MonadService> _logger;
    private readonly AsyncPolicy<GetTradeOutput> _genericPolicy;
    public MonadService(IOptions<EnvVariables> env,
        ILogger<MonadService> logger)
    {
        var monadRpc = env.Value.MonadRpcUrl;
        _web3 = new Web3(monadRpc);
        _contractAddress = env.Value.ContractAddress;
        _logger = logger;

        _genericPolicy = Policy<GetTradeOutput>
            .Handle<Nethereum.JsonRpc.Client.RpcClientUnknownException>()
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt - 1) + Jitterer.Next(0, 100)),
                onRetry: (outcome, timespan, attempt) =>
                {
                    if (outcome.Exception != null)
                        _logger.LogWarning(outcome.Exception, "Retry {Attempt} due to exception. Waiting {Delay}", attempt, timespan);
                    else
                        _logger.LogWarning("Retry {Attempt} due to unsuccessful result. Waiting {Delay}. Result: {Result}", attempt, timespan, outcome.Result);
                });
    }

    public async Task<GetTradeOutput> GetTradeDataAsync(BigInteger tradeId,
        CancellationToken cancellationToken = default)
    {
        return await _genericPolicy.ExecuteAsync(async ct =>
        {
            ct.ThrowIfCancellationRequested();

            var abi = await File.ReadAllTextAsync("Services/Monad/abi.json", ct);
            var contract = _web3.Eth.GetContract(abi, _contractAddress);

            var func = contract.GetFunction<GetTradeFunction>();

            var function = contract.GetFunction("trades");
            var callInput = function.CreateCallInput(tradeId);
            _logger.LogInformation("Trade id from monad service: {id}", tradeId);

            var rawHex = await _web3.Eth.Transactions.Call.SendRequestAsync(callInput);
            _logger.LogDebug("Raw result hex: {RawHex}", rawHex);

            var getTradeMsg = new GetTradeFunction { TradeId = tradeId };

            GetTradeOutput trade;
            try
            {
                trade = await func.CallDeserializingToObjectAsync<GetTradeOutput>(getTradeMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in deserialization of trade data for trade {TradeId}", tradeId);
                throw;
            }

            return trade;
        }, cancellationToken);
    }

    public async Task<string> GetTransactionInitiator(string txHash)
    {
        var transaction = await _web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(txHash);

        return transaction.From;
    }
}