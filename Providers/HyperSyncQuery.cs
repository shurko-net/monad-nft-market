using System.Text.Json;
using Microsoft.Extensions.Options;
using MonadNftMarket.Configuration;
using MonadNftMarket.Models.DTO.ContractEvents;
using MonadNftMarket.Models.DTO.HyperSync;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

namespace MonadNftMarket.Providers;

public class HyperSyncQuery : IHyperSyncQuery
{
    private readonly HttpClient _httpClient;
    private readonly string _hyperSyncQueryUrl;
    private readonly string _alternativeHyperSyncQueryUrl;
    private readonly string _monadRpcUrl;
    private readonly string _contractAddress;
    private readonly Web3 _web3;

    public HyperSyncQuery(IOptions<EnvVariables> env)
    {
        _httpClient = new HttpClient();
        var envValue = env.Value;
        
        _hyperSyncQueryUrl = envValue.HyperSyncQueryUrl;
        _alternativeHyperSyncQueryUrl = envValue.AlternativeHyperSyncQueryUrl;
        _monadRpcUrl = envValue.MonadRpcUrl;
        _contractAddress = envValue.ContractAddress;
        _web3 = new Web3(envValue.HyperSyncQueryUrl);
    }

    public async Task<Data> GetLogs(long nextBlock)
    {
        var payload = new
        {
            from_block = nextBlock + 1,
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
        
        var data = JsonSerializer.Deserialize<HyperSyncWrapper>(await response.Content.ReadAsStringAsync()
            , new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return data?.Data ?? new Data();
    }
}