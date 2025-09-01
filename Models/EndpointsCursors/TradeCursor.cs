using MonadNftMarket.Models.DTO;

namespace MonadNftMarket.Models.EndpointsCursors;

public sealed record TradeCursor
{
    public Guid LastId { get; set; }
    public TradeDirection Direction { get; set; }
}