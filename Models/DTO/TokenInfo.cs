using System.Text.Json.Serialization;

namespace MonadNftMarket.Models.DTO;

public class TokenInfo
{
    [JsonPropertyName("chainId")] public int ChainId { get; set; }
    [JsonPropertyName("contract")] public string Contract { get; set; } = string.Empty;
    [JsonPropertyName("tokenId")] public string TokenId { get; set; } = string.Empty;
    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("image")] public string Image { get; set; } = string.Empty;
    [JsonPropertyName("imageSmall")] public string ImageSmall { get; set; } = string.Empty;
    [JsonPropertyName("imageLarge")] public string ImageLarge { get; set; } = string.Empty;
    [JsonPropertyName("imageLarge")] public MetadataInfo MetadataInfo { get; set; } = null!;
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
    [JsonPropertyName("rarityScore")] public string RarityScore { get; set; } = string.Empty;
    [JsonPropertyName("rarityRank")] public string RarityRank { get; set; } = string.Empty;
    [JsonPropertyName("supply")] public string Supply { get; set; } = string.Empty;
    [JsonPropertyName("remainingSupply")] public string RemainingSupply { get; set; } = string.Empty;
    [JsonPropertyName("media")] public string Media { get; set; } = string.Empty;
    [JsonPropertyName("isFlagged")] public bool IsFlagged { get; set; }
    [JsonPropertyName("isSpam")] public bool IsSpam { get; set; }
    [JsonPropertyName("metadataDisabled")] public bool MetadataDisabled { get; set; }
    [JsonPropertyName("lastFlagUpdate")] public string LastFlagUpdate { get; set; } = string.Empty;
    [JsonPropertyName("lastFlagChange")] public string LastFlagChange { get; set; } = string.Empty;
    [JsonPropertyName("collection")] public CollectionInfo Collection { get; set; } = null!;
    
}