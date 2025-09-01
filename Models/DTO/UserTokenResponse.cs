namespace MonadNftMarket.Models.DTO;

public class UserTokenResponse<T> : PagedResult<T>
{
    public decimal TotalValue { get; set; }
    public int NftAmount { get; set; }
}