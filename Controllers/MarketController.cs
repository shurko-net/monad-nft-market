using MonadNftMarket.Services.Token;
using MonadNftMarket.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MonadNftMarket.Context;
using MonadNftMarket.Models.DTO;

namespace MonadNftMarket.Controllers;

[ApiController]
[Route("api/market")]
public class MarketController(
    IUserIdentity userIdentity,
    IMagicEdenProvider magicEdenProvider,
    ApiDbContext db) : ControllerBase
{
    [Authorize]
    [HttpGet("user-tokens")]
    public async Task<IActionResult> GetUserTokens()
    {
        var address = userIdentity.GetAddressByCookie(HttpContext);

        var response = await magicEdenProvider.GetUserTokensAsync(address);

        if (response.Count > 0)
            return Ok(response);

        return NotFound();
    }

    [HttpGet("market-listing")]
    public async Task<IActionResult> GetMarketListing(int page, int pageSize = 10)
    {
        var listings = await db.Listings
            .AsNoTracking()
            .Where(l => l.IsActive)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        var metadata = await magicEdenProvider
            .GetListingMetadataAsync(listings.Select(l => l.NftContractAddress ?? string.Empty).ToList(),
                listings.Select(l => l.TokenId).ToList());

        var response = listings.Zip(metadata, (l, m) => new
        {
            Listing = l,
            Metadata = m
        });
        
        return Ok(response);
    }

    [HttpGet("trades")]
    public async Task<IActionResult> GetTrades(int page, int pageSize = 10)
    {
        var trades = await db.Trades
            .AsNoTracking()
            .Where(t => t.IsActive)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        var result = new List<TradeMetadata>();
        
        foreach (var trade in trades)
        {
            result.Add(await magicEdenProvider.GetTradeMetadataAsync(trade));
        }

        return Ok(result);
    }
}