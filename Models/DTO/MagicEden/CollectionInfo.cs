using System.Text.Json.Serialization;

namespace MonadNftMarket.Models.MagicEden.DTO;

public class CollectionInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("isSpam")]
    public bool IsSpam { get; set; }

    [JsonPropertyName("metadataDisabled")]
    public bool MetadataDisabled { get; set; }

    [JsonPropertyName("openseaVerificationStatus")]
    public string OpenseaVerificationStatus { get; set; } = string.Empty;

    [JsonPropertyName("floorAskPrice")]
    public FloorAskPriceInfo FloorAskPrice { get; set; } = null!;

    [JsonPropertyName("royaltiesBps")]
    public int RoyaltiesBps { get; set; }

    [JsonPropertyName("royalties")]
    public List<RoyaltyInfo> Royalties { get; set; } = null!;

    [JsonPropertyName("lastAppraisalValue")]
    public string LastAppraisalValue { get; set; } = string.Empty;
}