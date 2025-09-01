namespace MonadNftMarket.Models.EndpointsCursors;

public record MarketListingCursor(
    bool ExcludeSelf,
    string? Seller = null,
    string SortBy = "id",
    string OrderBy = "desc",
    string? Search = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null);