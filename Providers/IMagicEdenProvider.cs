using MonadNftMarket.Models.DTO;

namespace MonadNftMarket.Providers;

public interface IMagicEdenProvider
{
    public Task<List<UserToken>> GetTokensAsync(string userAddress);
}