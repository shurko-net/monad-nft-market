namespace MonadNftMarket.Models.DTO;

public class HistoryResponse<T> : PagedResult<T>
{
    public int TotalPages { get; set; }
}