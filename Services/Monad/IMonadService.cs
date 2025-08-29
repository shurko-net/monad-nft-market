using System.Numerics;
using MonadNftMarket.Models.ContractOutput;

namespace MonadNftMarket.Services.Monad;

public interface IMonadService
{
    Task<GetTradeOutput> GetTradeDataAsync(BigInteger tradeId, CancellationToken cancellationToken = default);
    Task<string> GetTransactionInitiator(string txHash);
}