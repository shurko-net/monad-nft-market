using System.Numerics;
using MonadNftMarket.Models.DTO.HyperSync;

namespace MonadNftMarket.Providers;

public interface IHyperSyncQuery
{
    public Task<Root> GetLogs(BigInteger nextBlock);
}