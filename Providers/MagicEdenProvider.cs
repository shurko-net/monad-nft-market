using System.Numerics;
using MonadNftMarket.Models.DTO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MonadNftMarket.Configuration;
using MonadNftMarket.Models;
using MonadNftMarket.Models.DTO.MagicEden;

namespace MonadNftMarket.Providers;

public class MagicEdenProvider : IMagicEdenProvider
{
    private readonly string _userTokensUrl;
    private readonly string _tokensMetadataUrl;
    private static readonly HttpClient HttpClient = new();
    private readonly IMemoryCache _cache;
    private readonly ILogger<MagicEdenProvider> _logger;
    public MagicEdenProvider(
        IOptions<EnvVariables> env,
        IMemoryCache cache,
        ILogger<MagicEdenProvider> logger)
    {
        _userTokensUrl = env.Value.MagicEdenUserTokens;
        _tokensMetadataUrl = env.Value.MagicEdenTokensMetadata;
        _cache = cache;
        _logger = logger;
    }
    private string BuildUserTokensUrl(string userAddress)
    {
        if(string.IsNullOrEmpty(userAddress))
            throw new ArgumentException("userAddress is required", nameof(userAddress));

        return _userTokensUrl.Replace($"{{{nameof(userAddress)}}}", Uri.EscapeDataString(userAddress));
    }
    private string BuildTokensMetadataUrl(Peer peer)
    {
        if(peer.NftContracts.Count == 0 || peer.TokenIds.Count == 0)
            _logger.LogCritical("No BuildTokensMetadataUrl contracts found");
        
        var zip = peer.NftContracts.Zip(peer.TokenIds,
            (contract, id) => $"{contract}:{id}");
        
        var qsParts = zip.Select(t => "tokens=" + Uri.EscapeDataString(t));
        
        return _tokensMetadataUrl + "?" + string.Join("&", qsParts);
    }
    private string BuildTokensMetadataUrl(List<string> contracts, List<BigInteger> ids)
    {
        if(contracts.Count == 0 || ids.Count == 0)
            return string.Empty;
        
        var zip = contracts.Zip(ids,
            (contract, id) => $"{contract}:{id}");
        
        var qsParts = zip.Select(t => "tokens=" + Uri.EscapeDataString(t));
        
        return _tokensMetadataUrl + "?" + string.Join("&", qsParts);
    }
    private async Task<List<JsonDocument>> GetAllPagesAsync(string baseUrl)
    {
        var result = new List<JsonDocument>();

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            _logger.LogError("baseUrl is required in GetAllPagesAsync: " + baseUrl);
            return result;
        }
        
        string? continuation = null;
        
        do
        {
            var uriBuilder = new UriBuilder(baseUrl);
            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            if (!string.IsNullOrEmpty(continuation))
                query["continuation"] = continuation;
            uriBuilder.Query = query.ToString() ?? string.Empty;

            var finalUrl = uriBuilder.Uri.ToString();
            var responseStr = await HttpClient.GetStringAsync(finalUrl);
            _logger.LogCritical("Dude");

            if (string.IsNullOrWhiteSpace(responseStr))
                break;

            var jsonDoc = JsonDocument.Parse(responseStr);
            _logger.LogCritical($"JsonDoc: {responseStr}");
            result.Add(jsonDoc);

            continuation = jsonDoc.RootElement.TryGetProperty("continuation", out var contElem) &&
                           contElem.ValueKind == JsonValueKind.String
                ? contElem.GetString()
                : null;
        } while (!string.IsNullOrEmpty(continuation));
        
        return result;
    }

    private async Task<List<TokensResponse>> DeserializeMetadata(string url)
    {
        var allTokens = new List<TokensResponse>();

        if (string.IsNullOrWhiteSpace(url))
            return allTokens;
        
        var pages = await GetAllPagesAsync(url);

        try
        {
            foreach (var page in pages)
            {
                if (page.RootElement.TryGetProperty("tokens", out var tokensElement))
                {
                    var tokens = tokensElement.Deserialize<List<TokensResponse>>(new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new JsonStringEnumConverter() },
                        NumberHandling = JsonNumberHandling.AllowReadingFromString
                    });

                    if (tokens != null)
                        allTokens.AddRange(tokens);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in {nameof(DeserializeMetadata)}: {ex.Message}");
        }
        
        return allTokens;
    }

    private List<UserToken> ToUserToken(List<TokensResponse>? tokens)
    {
        var result = new List<UserToken>();

        if (tokens == null || tokens.Count == 0)
            return result;
        
        return tokens.Where(x => x.Token.Kind == "erc721").Select(x =>
        {
            var t = x.Token;

            return new UserToken
            {
                ContractAddress = t.Contract ?? string.Empty,
                TokenId = t.TokenId ?? string.Empty,
                Kind = t.Kind ?? string.Empty,
                Name = t.Name ?? string.Empty,
                ImageOriginal = t.MetadataInfo?.ImageOriginal
                                ?? t.Image
                                ?? string.Empty,
                Description = t.Description ?? string.Empty,
                LastPrice = t.Collection?.FloorAskPrice?.Amount?.Native ?? 0m
            };
        }).ToList();
    }

    public async Task<List<UserToken>> GetUserTokensAsync(string userAddress)
    {
        var cacheKey = $"{nameof(GetUserTokensAsync)}_{userAddress}";

        if (_cache.TryGetValue(cacheKey, out List<UserToken>? cached))
        {
            if (cached != null)
                return cached;
        }

        var url = BuildUserTokensUrl(userAddress);
        var allTokens = await DeserializeMetadata(url);
        
        var result = ToUserToken(allTokens);

        _cache.Set(
            cacheKey,
            result,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
                SlidingExpiration = TimeSpan.FromSeconds(30),
                Priority = CacheItemPriority.Normal
            });
        
        return result;
    }

    public async Task<TradeMetadata> GetTradeMetadataAsync(Trade trade)
    {
        var cacheKey = $"{nameof(GetTradeMetadataAsync)}_{trade.TradeId}";

        if (_cache.TryGetValue(cacheKey, out TradeMetadata? cached))
        {
            if (cached != null)
                return cached;
        }
        
        var fromTokens = await DeserializeMetadata(BuildTokensMetadataUrl(trade.From));
        var toTokens = await DeserializeMetadata(BuildTokensMetadataUrl(trade.To));

        var from = ToUserToken(fromTokens);
        var to = ToUserToken(toTokens);

        var result = new TradeMetadata
        {
            TradeId = trade.TradeId,
            FromAddress = trade.From.Address,
            ToAddress = trade.To.Address,
            From = from,
            To = to
        };
        
        _cache.Set(
            cacheKey,
            result,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
                SlidingExpiration = TimeSpan.FromSeconds(30),
                Priority = CacheItemPriority.Normal
            });
        
        return result;
    }
    
    public async Task<IReadOnlyDictionary<string, Metadata>> GetListingMetadataAsync(List<string> contracts, List<BigInteger> ids)
    {
        if (contracts.Count == 0 || ids.Count == 0)
            return new Dictionary<string, Metadata>(StringComparer.OrdinalIgnoreCase);
        
        var url = BuildTokensMetadataUrl(contracts, ids);
        if(string.IsNullOrEmpty(url))
            return new Dictionary<string, Metadata>(StringComparer.OrdinalIgnoreCase);
        
        var allTokens = await DeserializeMetadata(url);
        var userTokens = ToUserToken(allTokens);
        
        var dict = new Dictionary<string, Metadata>(StringComparer.OrdinalIgnoreCase);

        foreach (var t in userTokens)
        {
            if(string.IsNullOrEmpty(t.ContractAddress) || string.IsNullOrEmpty(t.TokenId))
                continue;
            
            var key = GetKey(t.ContractAddress, t.TokenId);

            if (!dict.ContainsKey(key))
            {
                dict[key] = new Metadata
                {
                    Kind = t.Kind,
                    Name = t.Name,
                    ImageOriginal = t.ImageOriginal,
                    Description = t.Description,
                    LastPrice = t.LastPrice
                };
            }
        }

        return dict;
    }
    
    private static string GetKey(string contract, string tokenId) =>
        $"{contract.ToLowerInvariant()}:{tokenId}";
}