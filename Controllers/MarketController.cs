using MonadNftMarket.Services.Token;
using MonadNftMarket.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MonadNftMarket.Context;
using MonadNftMarket.Models;
using MonadNftMarket.Models.DTO;

namespace MonadNftMarket.Controllers;

[ApiController]
[Route("api/market")]
public class MarketController(
    IUserIdentity userIdentity,
    IMagicEdenProvider magicEdenProvider,
    ApiDbContext db,
    ILogger<MarketController> logger) : ControllerBase
{
    [Authorize]
    [HttpGet("user-tokens")]
    public async Task<IActionResult> GetUserTokens(int page, int pageSize = 20)
    {
        var address = userIdentity.GetAddressByCookie(HttpContext);

        var userTokens = await magicEdenProvider.GetUserTokensAsync(address);

        page = Math.Max(1, page);
        pageSize = Math.Max(1, pageSize);

        var response = userTokens
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(new
        {
            response,
            TotalValue = userTokens.Sum(t => t.LastPrice ?? 0m),
            NftAmount = userTokens.Count
        });
    }

    [HttpGet("market-listing")]
    public async Task<IActionResult> GetMarketListing(
        [FromQuery] int page,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool excludeSelf = false)
    {
        var address = userIdentity.GetAddressByCookie(HttpContext);
        if (string.IsNullOrEmpty(address))
            excludeSelf = false;

        page = Math.Max(1, page);
        pageSize = Math.Max(1, pageSize);

        var query = db.Listings
            .AsNoTracking()
            .Where(l => l.IsActive);

        if (excludeSelf)
            query = query.Where(l => l.SellerAddress != address);

        var listings = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        if (listings.Count == 0)
            return Ok(Array.Empty<ListingResponse>());

        var metadata = await magicEdenProvider
            .GetListingMetadataAsync(listings.Select(l => l.NftContractAddress ?? string.Empty).ToList(),
                listings.Select(l => l.TokenId).ToList());

        var response = listings.Zip(metadata, (l, m) => new ListingResponse
        {
            ListingId = l.ListingId,
            ContractAddress = l.NftContractAddress,
            TokenId = l.TokenId,
            SellerAddress = l.SellerAddress,
            Price = l.Price,
            Metadata = new Metadata
            {
                Kind = m.Kind,
                Name = m.Name,
                ImageOriginal = m.ImageOriginal,
                Description = m.Description,
                LastPrice = m.LastPrice
            },
            IsOwnedByCurrentUser = !string.IsNullOrEmpty(address) && l.SellerAddress == address
        }).ToList();

        return Ok(response);
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
            .Where(t => t.IsActive)
            .Include(t => t.From)
            .Include(t => t.To)
            .AsSplitQuery()
            .OrderByDescending(t => t.EventMetadata.Timestamp)
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

        var result = new List<TradeResponse>();

        foreach (var trade in trades)
        {
            var metadata = await magicEdenProvider.GetTradeMetadataAsync(trade);

            result.Add(new TradeResponse
            {
                TradeId = metadata.TradeId,
                FromAddress = metadata.FromAddress,
                ToAddress = metadata.ToAddress,
                FromMetadata = metadata.From.Select(m => new Metadata
                {
                    Kind = m.Kind,
                    Name = m.Name,
                    ImageOriginal = m.ImageOriginal,
                    Description = m.Description,
                    LastPrice = m.LastPrice
                }),
                ToMetadata = metadata.To.Select(m => new Metadata
                {
                    Kind = m.Kind,
                    Name = m.Name,
                    ImageOriginal = m.ImageOriginal,
                    Description = m.Description,
                    LastPrice = m.LastPrice
                }),
                IsIncoming = metadata.ToAddress == address
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
        
        var history = await db.Notifications
            .AsNoTracking()
            .Where(n => n.UserAddress == address && 
                        (n.Type == NotificationType.TradeAccepted || n.Type == NotificationType.TradeRejected))
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return Ok(history);
    }
}