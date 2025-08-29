namespace MonadNftMarket.Models;

public enum EventStatus
{
    ListingCreated,
    ListingRemoved,
    ListingSold,
    ListingBought,
    TradeCreated,
    TradeAccepted,
    TradeRejected,
    TradeCompleted,
    TradeReceived
}