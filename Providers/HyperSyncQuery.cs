using System.Numerics;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MonadNftMarket.Configuration;
using MonadNftMarket.Models.DTO.HyperSync;
using Nethereum.Web3;

namespace MonadNftMarket.Providers;

public class HyperSyncQuery : IHyperSyncQuery
{
    private readonly HttpClient _httpClient;
    private readonly string _hyperSyncQueryUrl;
    private readonly string _contractAddress;
    private readonly Web3 _web3;
    private readonly int _blocksForConfirmation; 

    public HyperSyncQuery(IOptions<EnvVariables> env)
    {
        _httpClient = new HttpClient();
        var envValue = env.Value;
        
        _hyperSyncQueryUrl = envValue.HyperSyncQueryUrl;
        _contractAddress = envValue.ContractAddress;
        _blocksForConfirmation = envValue.BlocksForConfirmation;
        
        _web3 = new Web3(envValue.MonadRpcUrl);
    }

    public async Task<Root> GetLogs(BigInteger fromBlock)
    {
        var latestBlock = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
        var payload = new
        {
            from_block = (ulong)fromBlock,
            to_block = (ulong)(latestBlock.Value - _blocksForConfirmation),
            logs = new[]
            {
                new { address = new[] { _contractAddress } }
            },
            field_selection = new
            {
                block = new[] { "number", "timestamp", "hash" },
                log = new[]
                {
                    "block_number", "log_index", "transaction_index", "data", "address", "topic0", "topic1", "topic2",
                    "topic3"
                },
                transaction = new[] { "block_number", "transaction_index", "hash", "from", "to", "value", "input" }
            }
        };
        
        var response = await _httpClient.PostAsJsonAsync(_hyperSyncQueryUrl, payload);
        
        var data = JsonSerializer.Deserialize<Root>(await response.Content.ReadAsStringAsync()
            , new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return data ?? new Root();
    }
}