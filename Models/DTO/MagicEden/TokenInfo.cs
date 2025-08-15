using System.Text.Json.Serialization;
using MonadNftMarket.Models.MagicEden.DTO;

namespace MonadNftMarket.Models.DTO.MagicEden;

public class TokenInfo
{
    [JsonPropertyName("chainId")] public int? ChainId { get; set; }
    [JsonPropertyName("contract")] public string? Contract { get; set; }
    [JsonPropertyName("tokenId")] public string? TokenId { get; set; }
    [JsonPropertyName("kind")] public string? Kind { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("image")] public string? Image { get; set; }
    [JsonPropertyName("imageSmall")] public string? ImageSmall { get; set; }
    [JsonPropertyName("imageLarge")] public string? ImageLarge { get; set; }
    [JsonPropertyName("metadata")] public MetadataInfo? MetadataInfo { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("rarityScore")] public decimal? RarityScore { get; set; }
    [JsonPropertyName("rarityRank")] public decimal? RarityRank { get; set; }
    [JsonPropertyName("supply")] public string? Supply { get; set; }
    [JsonPropertyName("remainingSupply")] public string? RemainingSupply { get; set; }
    [JsonPropertyName("media")] public string? Media { get; set; }
    [JsonPropertyName("isFlagged")] public bool? IsFlagged { get; set; }
    [JsonPropertyName("isSpam")] public bool? IsSpam { get; set; }
    [JsonPropertyName("metadataDisabled")] public bool? MetadataDisabled { get; set; }
    [JsonPropertyName("lastFlagUpdate")] public string? LastFlagUpdate { get; set; }
    [JsonPropertyName("lastFlagChange")] public string? LastFlagChange { get; set; }
    [JsonPropertyName("collection")] public CollectionInfo? Collection { get; set; }
}