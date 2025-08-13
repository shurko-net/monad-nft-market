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
    private static readonly HttpClient HttpClient = new HttpClient();
    private readonly IMemoryCache _cache;
    public MagicEdenProvider(
        IOptions<EnvVariables> env,
        IMemoryCache cache)
    {
        _userTokensUrl = env.Value.MagicEdenUserTokens;
        _tokensMetadataUrl = env.Value.MagicEdenTokensMetadata;
        _cache = cache;
    }
    private string BuildUserTokensUrl(string userAddress)
    {
        if(string.IsNullOrEmpty(userAddress))
            throw new ArgumentException("userAddress is required", nameof(userAddress));

        return _userTokensUrl.Replace($"{{{nameof(userAddress)}}}", Uri.EscapeDataString(userAddress));
    }
    private string BuildTokensMetadataUrl(Peer peer)
    {
        var zip = peer.NftContracts.Zip(peer.TokenIds,
            (contract, id) => $"{contract}:{id}");
        
        var qsParts = zip.Select(t => "tokens=" + Uri.EscapeDataString(t));
        
        return _tokensMetadataUrl + "?" + string.Join("&", qsParts);
    }
    private string BuildTokensMetadataUrl(List<string> contracts, List<BigInteger> ids)
    {
        var zip = contracts.Zip(ids,
            (contract, id) => $"{contract}:{id}");
        
        var qsParts = zip.Select(t => "tokens=" + Uri.EscapeDataString(t));
        
        return _tokensMetadataUrl + "?" + string.Join("&", qsParts);
    }
    private static async Task<List<JsonDocument>> GetAllPagesAsync(string baseUrl)
    {
        var result = new List<JsonDocument>();
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

            if (string.IsNullOrWhiteSpace(responseStr))
                break;

            var jsonDoc = JsonDocument.Parse(responseStr);
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
        var pages = await GetAllPagesAsync(url);

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
        
        return allTokens;
    }

    private List<UserToken> ToUserToken(List<TokensResponse> tokens)
    {
        return tokens.Select(token =>
        {
            var t = token.Token;
            
            var lastPrice = t.Collection.FloorAskPrice.Amount.Native;
            
            var image = t.MetadataInfo.ImageOriginal;
            var contract = t.Contract;
            var tokenId = t.TokenId;
            var kind = t.Kind;
            var name = t.Name;
            var description = t.Description;

            return new UserToken
            {
                ContractAddress = contract,
                TokenId = tokenId,
                Kind = kind,
                Name = name,
                Description = description,
                LastPrice = lastPrice,
                ImageOriginal = image
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
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
                SlidingExpiration = TimeSpan.FromMinutes(5),
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
            FromAddress = trade.From.Address ?? string.Empty,
            ToAddress = trade.To.Address ?? string.Empty,
            From = from,
            To = to
        };
        
        _cache.Set(
            cacheKey,
            result,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
                SlidingExpiration = TimeSpan.FromMinutes(5),
                Priority = CacheItemPriority.Normal
            });
        
        return result;
    }

    public async Task<List<UserToken>> GetListingMetadataAsync(List<string> contracts, List<BigInteger> ids)
    {
        var url = BuildTokensMetadataUrl(contracts, ids);
        var allTokens = await DeserializeMetadata(url);   

        return ToUserToken(allTokens);
    }
}