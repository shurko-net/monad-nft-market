using System.Text.Json.Serialization;
using MonadNftMarket.Models.MagicEden.DTO;

namespace MonadNftMarket.Models.DTO.MagicEden;

public class CollectionInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("slug")]
    public string? Slug { get; set; }

    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; } 

    [JsonPropertyName("isSpam")]
    public bool IsSpam { get; set; }

    [JsonPropertyName("metadataDisabled")]
    public bool MetadataDisabled { get; set; }

    [JsonPropertyName("openseaVerificationStatus")]
    public string? OpenseaVerificationStatus { get; set; }

    [JsonPropertyName("floorAskPrice")]
    public FloorAskPriceInfo FloorAskPrice { get; set; } = null!;

    [JsonPropertyName("royaltiesBps")]
    public int RoyaltiesBps { get; set; }

    [JsonPropertyName("royalties")]
    public List<RoyaltyInfo> Royalties { get; set; } = null!;

    [JsonPropertyName("lastAppraisalValue")]
    public string LastAppraisalValue { get; set; } = string.Empty;
}