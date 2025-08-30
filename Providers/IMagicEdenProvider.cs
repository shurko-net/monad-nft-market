using System.Numerics;
using MonadNftMarket.Models;
using MonadNftMarket.Models.DTO;

namespace MonadNftMarket.Providers;

public interface IMagicEdenProvider
{
    public Task<List<UserToken>> GetUserTokensAsync(string userAddress, bool sortByDesc);
    public Task<IReadOnlyDictionary<string, Metadata>> GetListingMetadataAsync(List<string> contracts, List<BigInteger> ids);
}