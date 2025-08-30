namespace MonadNftMarket.Models.DTO;

public class Metadata
{
    public string Kind { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ImageOriginal { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? Price { get; set; }
}