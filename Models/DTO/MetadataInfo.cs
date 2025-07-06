using System.Text.Json.Serialization;

namespace MonadNftMarket.Models.DTO;

public class MetadataInfo
{
    [JsonPropertyName("imageOriginal")]
    public string ImageOriginal { get; set; } = string.Empty;

    [JsonPropertyName("imageMimeType")]
    public string ImageMimeType { get; set; } = string.Empty;

    [JsonPropertyName("tokenURI")]
    public string TokenURI { get; set; } = string.Empty;
}