using MonadNftMarket.Models.DTO.HyperSync;

namespace MonadNftMarket.Providers;

public interface IHyperSyncQuery
{
    public Task<Data> GetLogs(long nextBlock);
}