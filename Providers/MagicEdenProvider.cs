using MonadNftMarket.Models.MagicEden.DTO;
using MonadNftMarket.Models.DTO;
using RestSharp;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace MonadNftMarket.Providers;

public class MagicEdenProvider : IMagicEdenProvider
{
    public async Task<List<UserToken>> GetTokensAsync(string userAddress)
    {
        var allTokens = new List<TokenOwnership>();
        string? continuation = null;

        var baseUrl = $"https://api-mainnet.magiceden.dev/v3/rtp/monad-testnet/users/{userAddress}/tokens/v7" +
                      "?normalizeRoyalties=false&sortBy=acquiredAt&sortDirection=desc&limit=200" +
                      "&includeTopBid=false&includeAttributes=false" +
                      "&includeLastSale=false&includeRawData=false&filterSpamTokens=false&useNonFlaggedFloorAsk=false";

        var client = new RestClient();

        do
        {
            var url = baseUrl;
            if (!string.IsNullOrEmpty(continuation))
            {
                url += $"&continuation={Uri.EscapeDataString(continuation)}";
            }

            var request = new RestRequest(url);
            request.AddHeader("accept", "*/*");

            var response = await client.GetAsync(request);

            if (string.IsNullOrWhiteSpace(response.Content))
                break;

            var jsonDoc = JsonDocument.Parse(response.Content);

            if (jsonDoc.RootElement.TryGetProperty("tokens", out var tokensElement))
            {
                var tokens = JsonSerializer.Deserialize<List<TokenOwnership>>(tokensElement, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() },
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                });

                if (tokens != null)
                    allTokens.AddRange(tokens);
            }

            if (jsonDoc.RootElement.TryGetProperty("continuation", out var continuationElement) &&
                continuationElement.ValueKind == JsonValueKind.String)
            {
                continuation = continuationElement.GetString();
            }
            else
            {
                continuation = null;
            }

        } while (!string.IsNullOrEmpty(continuation));

        List<UserToken> userTokens = new();

        foreach (var token in allTokens)
        {
            userTokens.Add(new UserToken
            {
                Contract = token.Token?.Contract ?? string.Empty,
                TokenId = token.Token?.TokenId ?? string.Empty,
                Kind = token.Token?.Kind ?? string.Empty,
                Name = token.Token?.Name ?? string.Empty,
                Description = token.Token?.Description ?? string.Empty,
                LastPrice = token.Token?.Collection?.FloorAskPrice?.Amount?.Native ?? decimal.Zero,
                ImageOriginal = token.Token?.MetadataInfo?.ImageOriginal ?? string.Empty,
            });
        }

        return userTokens;
    }
}