using System.Text.Json.Serialization;

namespace MonadNftMarket.Models.MagicEden.DTO;

public class MetadataInfo
{
    [JsonPropertyName("imageOriginal")]
    public string? ImageOriginal { get; set; }

    [JsonPropertyName("imageMimeType")]
    public string? ImageMimeType { get; set; }

    [JsonPropertyName("tokenURI")]
    public string? TokenUri { get; set; }
}