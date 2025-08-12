using System.Numerics;
using MonadNftMarket.Models;
using MonadNftMarket.Models.DTO;

namespace MonadNftMarket.Providers;

public interface IMagicEdenProvider
{
    public Task<List<UserToken>> GetUserTokensAsync(string userAddress);
    public Task<TradeMetadata> GetTradeMetadataAsync(Trade trade);
    public Task<List<UserToken>> GetListingMetadataAsync(List<string> contracts, List<BigInteger> ids);
}