using MonadNftMarket.Models.DTO;

namespace MonadNftMarket.Providers;

public interface IMagicEdenProvider
{
    public Task<List<TokenOwnership>> GetTokensAsync(string userAddress);
}