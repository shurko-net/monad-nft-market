using System.Numerics;
using Microsoft.EntityFrameworkCore;
using MonadNftMarket.Context;
using MonadNftMarket.Providers;

namespace MonadNftMarket.Services.UpdateNftMetadata;

public class UpdateMetadata : IUpdateMetadata
{
    private readonly IMagicEdenProvider _magicEdenProvider;
    private readonly ApiDbContext _db;

    public UpdateMetadata(
        IMagicEdenProvider magicEdenProvider,
        ApiDbContext db)
    {
        _magicEdenProvider = magicEdenProvider;
        _db = db;
    }
    
    public async Task UpdateMetadataAsync(
        List<string> contractAddresses,
        List<BigInteger> tokenIds)
    {
        var metadata = await _magicEdenProvider
            .GetListingMetadataAsync(contractAddresses, tokenIds);

        foreach (var data in contractAddresses.Zip(tokenIds))
        {
            var key = MakeKey(data.First, data.Second);
            
            if(!metadata.TryGetValue(key, out var mt))
                continue;

            await _db.Listings
                .Where(n => n.NftContractAddress == data.First
                            && n.TokenId == data.Second)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.NftMetadata.Kind, _ => mt.Kind)
                    .SetProperty(n => n.NftMetadata.Name, _ => mt.Name)
                    .SetProperty(n => n.NftMetadata.ImageOriginal, _ => mt.ImageOriginal)
                    .SetProperty(n => n.NftMetadata.Description, _ => mt.Description)
                    .SetProperty(n => n.NftMetadata.Price, _ => mt.Price ?? 0m)
                    .SetProperty(n => n.NftMetadata.LastUpdated, _ => DateTime.UtcNow));
        }
    }

    private string MakeKey(string contract, BigInteger tokenId)
        => $"{contract.ToLowerInvariant()}:{tokenId}";
}