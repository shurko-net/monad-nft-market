namespace MonadNftMarket.Models.DTO;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public bool HasMore { get; set; }
    public string? NextCursor { get; set; }
}