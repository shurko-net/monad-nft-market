using System.Text.Json.Serialization;

namespace MonadNftMarket.Models.DTO.MagicEden;

public class TokensResponse
{
    [JsonPropertyName("token")] public TokenInfo Token { get; set; } = null!;
    [JsonPropertyName("ownership")] public OwnershipInfo Ownership { get; set; } = null!;
    [JsonPropertyName("continuation")] public string Continuation { get; set; } = string.Empty;
}