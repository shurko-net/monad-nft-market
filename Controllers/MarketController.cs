using MonadNftMarket.Services.Token;
using MonadNftMarket.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MonadNftMarket.Context;
using MonadNftMarket.Models;
using MonadNftMarket.Models.DTO;
using MonadNftMarket.Models.EndpointsCursors;
using MonadNftMarket.Services;
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
        [FromQuery] int pageSize = 20,
        [FromQuery] string? nextCursor = null,
        [FromQuery] string orderBy = "desc")
    {
        pageSize = Math.Max(1, pageSize);
        var address = userIdentity.GetAddressByCookie(HttpContext);
        
        Enum.TryParse<OrderDirection>(orderBy, true, out var dir);
        
        IEnumerable<UserToken> userTokens = [];
        
        if (!string.IsNullOrEmpty(nextCursor))
        {
            var data = CursorService.Decode<UserTokenCursor>(nextCursor);

            if (data != null)
            {
                dir = data.OrderBy;
                userTokens = await magicEdenProvider.GetUserTokensAsync(address, dir == OrderDirection.Desc);
                
                userTokens = dir switch
                {
                    OrderDirection.Desc => userTokens.Where(u => u.AcquiredAt < data.AcquiredAt),
                    OrderDirection.Asc => userTokens.Where(u => u.AcquiredAt > data.AcquiredAt),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
        else
        {
            userTokens = await magicEdenProvider.GetUserTokensAsync(address, dir == OrderDirection.Desc);
        }

        var fetched = userTokens
            .Take(pageSize + 1)
            .ToList();
        
        var items = fetched.Take(pageSize).ToList();
        var hasMore = fetched.Count > pageSize;

        return Ok(new UserTokenResponse<UserToken>
        {
            Items = items,
            HasMore = hasMore,
            NextCursor = hasMore
                ? CursorService.Encode(new UserTokenCursor(items[^1].AcquiredAt, dir))
                : null,
            TotalValue = userTokens.ToList().Sum(u => u.Price ?? 0m),
            NftAmount = userTokens.ToList().Count
        });
    }

    [HttpGet("market-listing")]
    public async Task<IActionResult> GetMarketListing(
        [FromQuery] int pageSize = 10,
        [FromQuery] bool excludeSelf = false,
        [FromQuery] string? seller = null,
        [FromQuery] string sortBy = "id",
        [FromQuery] string orderBy = "desc",
        [FromQuery] string? search = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string? nextCursor = null)
    {
        Enum.TryParse<OrderDirection>(orderBy, true, out var dir);
        
        if (minPrice > maxPrice)
        {
            ModelState.AddModelError("Price", "minPrice can`t be greater maxPrice");
            return BadRequest(ModelState);
        }
        pageSize = Math.Max(1, pageSize);
        
        var maxDbPrice = await db.Listings.MaxAsync(p => p.Price);

        if (maxPrice.HasValue && maxPrice > maxDbPrice)
            maxPrice = maxDbPrice;
        
        var address = userIdentity.GetAddressByCookie(HttpContext);
        var cutoff = DateTime.UtcNow.AddDays(-7);
        if (string.IsNullOrEmpty(address))
            excludeSelf = false;
        
        var query = db.Listings
            .AsNoTracking()
            .Where(l => l.Status == EventStatus.ListingCreated)
            .AsQueryable();

        if (excludeSelf)
            query = query.Where(l => l.SellerAddress != address);
        
        if(!string.IsNullOrEmpty(seller))
            query = query.Where(l => EF.Functions.ILike(l.SellerAddress, seller));

        query = sortBy.ToLowerInvariant() switch
        {
            "id" => dir == OrderDirection.Desc
                ? query.OrderByDescending(l => l.Id)
                : query.OrderBy(l => l.Id),
            "contractaddress" => dir == OrderDirection.Desc
                ? query.OrderByDescending(l => l.NftContractAddress)
                : query.OrderBy(l => l.NftContractAddress),
            "tokenid" => orderBy == "desc" ? query.OrderByDescending(l => l.TokenId) : query.OrderBy(l => l.TokenId),
            "selleraddress" => dir == OrderDirection.Desc
                ? query.OrderByDescending(l => l.SellerAddress)
                : query.OrderBy(l => l.SellerAddress),
            "price" => dir == OrderDirection.Desc ? query.OrderByDescending(l => l.Price) : query.OrderBy(l => l.Price),
            "name" => dir == OrderDirection.Desc ? query.OrderByDescending(l => l.NftMetadata.Name) : query.OrderBy(l => l.NftMetadata.Name),
            _ => dir == OrderDirection.Desc ? query.OrderByDescending(l => l.Id) : query.OrderBy(l => l.Id)
        };

        if (!string.IsNullOrEmpty(search))
        {
            var term = search.Trim();
            if (term.Length > 60)
                term = term[..60];
            
            var pattern = $"%{term}%";
            query = query.Where(l => EF.Functions.ILike(l.NftMetadata.Name, pattern));
        }
        
        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        var listings = await query
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
                    Price = l.NftMetadata.Price
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
                outdatedPairs.Select(o => o.TokenId).ToList());
        
        return Ok(listings);
    }

    [Authorize]
    [HttpGet("trades")]
    public async Task<IActionResult> GetTrades(
        [FromQuery] int pageSize = 10,
        [FromQuery] string? nextCursor = null,
        [FromQuery] string direction = "all")
    {
        Enum.TryParse<TradeDirection>(direction, true, out var dir);
        
        pageSize = Math.Max(1, pageSize);

        var address = userIdentity.GetAddressByCookie(HttpContext);
        
        var baseQuery = db.Trades
            .AsNoTracking()
            .AsSplitQuery()
            .OrderByDescending(t => t.Id)
            .Where(t => t.Status == EventStatus.TradeCreated)
            .Include(t => t.From)
            .Include(t => t.To)
            .AsQueryable();

        if (!string.IsNullOrEmpty(nextCursor))
        {
            var data = CursorService.Decode<TradeCursor>(nextCursor);

            if (data != null)
            {
                baseQuery = baseQuery.Where(t => t.Id < data.LastId);
                
                dir = data.Direction;
            }
        }
        
        baseQuery = dir switch
        {
            TradeDirection.All => baseQuery.Where(t => t.From.Address == address || t.To.Address == address),
            TradeDirection.Incoming => baseQuery.Where(t => t.To.Address == address),
            TradeDirection.Outgoing => baseQuery.Where(t => t.From.Address == address),
            _ => baseQuery
        };

        var fetched = await baseQuery
            .Take(pageSize + 1)
            .ToListAsync();
        
        var hasMore = fetched.Count > pageSize;
        var trades = fetched.Take(pageSize).ToList();

        if (trades.Count == 0)
            return Ok(Array.Empty<TradeResponse>());

        var allIds = trades.SelectMany(t => t.ListingIds).Distinct().ToArray();

        var metadata = await db.Listings
            .OrderByDescending(l => l.Id)
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
                    trade.From.TokenIds.ToList());
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
        
        return Ok(new PagedResult<TradeResponse>
        {
            Items = result,
            HasMore = hasMore,
            NextCursor = hasMore ? 
                CursorService.Encode(new TradeCursor(trades[^1].Id, dir))
                :
                null
        });
    }

    [Authorize]
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int pageSize = 10,
        [FromQuery] string? nextCursor = null)
    {
        pageSize = Math.Max(1, pageSize);

        var address = userIdentity.GetAddressByCookie(HttpContext);

        var historyQuery = db.History
            .AsNoTracking()
            .OrderByDescending(h => h.Id)
            .AsQueryable();
        
        if (!string.IsNullOrEmpty(nextCursor))
        {
            var data = CursorService.Decode<TradeCursor>(nextCursor);

            if (data != null)
            {
                historyQuery = historyQuery.Where(t => t.Id < data.LastId);
            }
        }

        var fetched = await historyQuery
            .Where(h => h.FromAddress == address &&
                        h.Status != EventStatus.TradeCompleted && h.Status != EventStatus.ListingRemoved
                        && h.Status != EventStatus.ListingCreated)
            .Select(h => new HistoryDto
            {
                HistoryId = h.Id,
                UserAddress = h.FromAddress,
                Status = (h.Status == EventStatus.ListingBought
                          || h.Status == EventStatus.ListingSold
                          || h.Status == EventStatus.TradeAccepted
                          || h.Status == EventStatus.TradeCompleted)
                    ? HistoryStatus.Success
                    : (h.Status == EventStatus.TradeCreated
                       || h.Status == EventStatus.TradeReceived)
                        ? HistoryStatus.Pending
                        : (h.Status == EventStatus.ListingRemoved
                           || h.Status == EventStatus.TradeRejected)
                            ? HistoryStatus.Reject
                            : HistoryStatus.Unknown,
                ListingId = h.ListingId,
                TradeId = h.TradeId,
                Trade = h.Trade,
                Listings = h.Listing,
                Metadata = h.EventMetadata
            })
            .Take(pageSize + 1)
            .ToListAsync();
        

        var totalItems = await db.History.CountAsync(h => h.FromAddress == address &&
                                                          h.Status != EventStatus.TradeCompleted &&
                                                          h.Status != EventStatus.ListingRemoved
                                                          && h.Status != EventStatus.ListingCreated);
        var hasMore = fetched.Count > pageSize;
        var history = fetched.Take(pageSize).ToList();
        
        return Ok(new HistoryResponse<HistoryDto>
        {
            Items = history,
            HasMore = hasMore,
            NextCursor = hasMore ? 
                CursorService.Encode(new HistoryCursor(history[^1].HistoryId))
                :
                null,
            TotalPages = (totalItems + pageSize - 1) / pageSize
        });
    }
}