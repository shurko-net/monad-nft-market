using System.Numerics;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MonadNftMarket.Configuration;
using MonadNftMarket.Models.DTO.HyperSync;

namespace MonadNftMarket.Providers;

public class HyperSyncQuery : IHyperSyncQuery
{
    private readonly HttpClient _httpClient;
    private readonly string _hyperSyncQueryUrl;
    private readonly string _alternativeHyperSyncQueryUrl;
    private readonly string _contractAddress;

    public HyperSyncQuery(IOptions<EnvVariables> env)
    {
        _httpClient = new HttpClient();
        var envValue = env.Value;
        
        _hyperSyncQueryUrl = envValue.HyperSyncQueryUrl;
        _alternativeHyperSyncQueryUrl = envValue.AlternativeHyperSyncQueryUrl;
        _contractAddress = envValue.ContractAddress;
    }

    public async Task<Root> GetLogs(BigInteger nextBlock)
    {
        Console.WriteLine($"Next block in GetLogs: {nextBlock.ToString()}");
        var payload = new
        {
            from_block = (ulong)nextBlock,
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