using System.Numerics;
using MonadNftMarket.Models.ContractOutput;

namespace MonadNftMarket.Services.Monad;

public interface IMonadService
{
    Task<GetTradeOutput> GetTradeData(BigInteger tradeId, CancellationToken cancellationToken = default);
}