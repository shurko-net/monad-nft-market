using Microsoft.Extensions.Options;
using MonadNftMarket.Configuration;
using MonadNftMarket.Models.DTO.HyperSync;

namespace MonadNftMarket.Providers;

public class HyperSyncQuery(IOptions<EnvVariables> env) : IHyperSyncQuery
{
    private readonly HttpClient _httpClient = new();
    private readonly string _hyperSyncQueryUrl = env.Value.HyperSyncQueryUrl;
    private readonly string _alternativeHyperSyncQueryUrl = env.Value.AlternativeHyperSyncQueryUrl;
    private readonly string _monadRpcUrl = env.Value.MonadRpcUrl;

    public Task<Data> GetLogs(long nextBlock)
    {
        throw new NotImplementedException();
    }
}