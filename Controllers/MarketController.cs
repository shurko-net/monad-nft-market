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
    ApiDbContext db) : ControllerBase
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
    public async Task<IActionResult> GetMarketListing(int page, int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Max(1, pageSize);
        
        var listings = await db.Listings
            .AsNoTracking()
            .Where(l => l.IsActive)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        if(listings.Count == 0)
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
            }
        }).ToList();
        
        return Ok(response);
    }

    [HttpGet("trades")]
    public async Task<IActionResult> GetTrades(int page, int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Max(1, pageSize);
        
        var trades = await db.Trades
            .AsNoTracking()
            .Where(t => t.IsActive)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
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
            });
        }

        return Ok(result);
    }
}