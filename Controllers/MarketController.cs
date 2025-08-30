using MonadNftMarket.Services.Token;
using MonadNftMarket.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MonadNftMarket.Context;
using MonadNftMarket.Models;
using MonadNftMarket.Models.DTO;
using MonadNftMarket.Services.UpdateNftMetadata;

namespace MonadNftMarket.Controllers;

[ApiController]
[Route("api/market")]
public class MarketController(
    IUserIdentity userIdentity,
    IMagicEdenProvider magicEdenProvider,
    IUpdateMetadata updateMetadata,
    ApiDbContext db) : ControllerBase
{
    [Authorize]
    [HttpGet("user-tokens")]
    public async Task<IActionResult> GetUserTokens(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool sortByDesc = true)
    {
        var address = userIdentity.GetAddressByCookie(HttpContext);

        var userTokens = await magicEdenProvider.GetUserTokensAsync(address, sortByDesc);

        page = Math.Max(1, page);
        pageSize = Math.Max(1, pageSize);

        var response = userTokens
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(new
        {
            response,
            TotalValue = userTokens.Sum(t => t.Price ?? 0m),
            NftAmount = userTokens.Count
        });
    }

    [HttpGet("market-listing")]
    public async Task<IActionResult> GetMarketListing(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool excludeSelf = false,
        [FromQuery] string? seller = null,
        [FromQuery] bool sortByDesc = true)
    {
        var address = userIdentity.GetAddressByCookie(HttpContext);
        var cutoff = DateTime.UtcNow.AddDays(-7);
        if (string.IsNullOrEmpty(address))
            excludeSelf = false;

        page = Math.Max(1, page);
        pageSize = Math.Max(1, pageSize);
        
        var query = db.Listings
            .AsNoTracking()
            .Where(l => l.Status == EventStatus.ListingCreated);

        if (excludeSelf)
            query = query.Where(l => l.SellerAddress != address);
        
        if(!string.IsNullOrEmpty(seller))
            query = query.Where(l => EF.Functions.ILike(l.SellerAddress, seller));
        
        query = sortByDesc ? 
            query.OrderByDescending(l => l.Id) :
            query.OrderBy(l => l.Id);

        var listings = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new ListingResponse
            {
                ListingId = l.ListingId,
                ContractAddress = l.NftContractAddress,
                TokenId = l.TokenId,
                SellerAddress = l.SellerAddress,
                Price = l.Price,
                Metadata = new ()
                {
                    Kind = l.NftMetadata.Kind,
                    Name = l.NftMetadata.Name,
                    ImageOriginal = l.NftMetadata.ImageOriginal,
                    Description = l.NftMetadata.Description,
                    Price = l.NftMetadata.LastPrice
                },
                IsOwnedByCurrentUser = l.SellerAddress == address,
                Status = l.Status,
                LastUpdated = l.NftMetadata.LastUpdated
            })
            .ToListAsync();

        if (listings.Count == 0)
            return Ok(Array.Empty<ListingResponse>());
        
        var outdatedPairs = listings
            .Where(x => x.LastUpdated < cutoff)
            .Select(x => new {x.ContractAddress, x.TokenId})
            .Distinct()
            .ToList();
        
        if(outdatedPairs.Count > 0)
            await updateMetadata.UpdateMetadataAsync(
                outdatedPairs.Select(o => o.ContractAddress).ToList()!,
                outdatedPairs.Select(o => o.TokenId).ToList(), sortByDesc);
        
        return Ok(listings);
    }

    [Authorize]
    [HttpGet("trades")]
    public async Task<IActionResult> GetTrades(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string direction = "all")
    {
        if (!Enum.TryParse<TradeDirection>(direction, true, out var dir))
            return Ok(Array.Empty<TradeResponse>());

        page = Math.Max(1, page);
        pageSize = Math.Max(1, pageSize);

        var address = userIdentity.GetAddressByCookie(HttpContext);
        
        var baseQuery = db.Trades
            .AsNoTracking()
            .Where(t => t.Status == EventStatus.TradeCreated)
            .Include(t => t.From)
            .Include(t => t.To)
            .AsSplitQuery()
            .OrderByDescending(t => t.TradeId)
            .AsQueryable();

        baseQuery = dir switch
        {
            TradeDirection.All => baseQuery.Where(t => t.From.Address == address || t.To.Address == address),
            TradeDirection.Incoming => baseQuery.Where(t => t.To.Address == address),
            TradeDirection.Outgoing => baseQuery.Where(t => t.From.Address == address),
            _ => baseQuery
        };

        var trades = await baseQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        if (trades.Count == 0)
            return Ok(Array.Empty<TradeResponse>());

        var allIds = trades.SelectMany(t => t.ListingIds).Distinct().ToArray();

        var metadata = await db.Listings
            .Where(l => allIds.Contains(l.ListingId))
            .Select(l => new TradeMetadataDto
            {
                ListingId = l.ListingId,
                Kind = l.NftMetadata.Kind,
                NftContractAddress = l.NftMetadata.NftContractAddress,
                TokenId = l.NftMetadata.TokenId,
                Description = l.NftMetadata.Kind,
                ImageOriginal = l.NftMetadata.ImageOriginal,
                Price = l.Price
            })
            .ToDictionaryAsync(x => x.ListingId);

        var tradeMetadataByTradeId = trades
            .ToDictionary(
                t => t.TradeId,
                t => t.ListingIds
                    .Select(id => metadata.GetValueOrDefault(id))
                    .Where(dto => dto != null)
                    .ToList());

        var result = new List<TradeResponse>();
        
        foreach (var trade in trades)
        {
            var fromMeta = await magicEdenProvider
                .GetListingMetadataAsync(trade.From.NftContracts.ToList(),
                    trade.From.TokenIds.ToList(), true);
            tradeMetadataByTradeId.TryGetValue(trade.TradeId, out var toMeta);
            
            result.Add(new TradeResponse
            {
                TradeId = trade.TradeId,
                FromAddress = trade.From.Address,
                ToAddress = trade.To.Address,
                FromMetadata = fromMeta.Values
                    .Select(m => new TradeMetadataDto
                    {
                        Kind = m.Kind,
                        Description = m.Description,
                        ImageOriginal = m.ImageOriginal,
                        Price = m.Price ?? 0m
                    }),
                ToMetadata = toMeta!,
                IsIncoming = trade.To.Address == address,
                Status = trade.Status
            });
        }
        
        return Ok(result);
    }

    [Authorize]
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Max(1, pageSize);

        var address = userIdentity.GetAddressByCookie(HttpContext);

        var history = await db.History
            .AsNoTracking()
            .Where(h => h.UserAddress == address)
            .Select(h => new HistoryDto
            {
                UserAddress = h.UserAddress,
                Status = h.Status,
                ListingId = h.ListingId,
                TradeId = h.TradeId,
                Metadata = h.EventMetadata
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return Ok(history);
    }
}