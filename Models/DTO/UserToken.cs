namespace MonadNftMarket.Models.DTO;

public class UserToken
{
    public string ContractAddress { get; set; } = string.Empty;
    public string TokenId { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ImageOriginal { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? LastPrice { get; set; }
}