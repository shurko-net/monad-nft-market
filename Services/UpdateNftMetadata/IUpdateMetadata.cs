using System.Numerics;

namespace MonadNftMarket.Services.UpdateNftMetadata;

public interface IUpdateMetadata
{
    Task UpdateMetadataAsync(List<string> contractAddresses, List<BigInteger> tokenIds, bool sortByDesc);
}