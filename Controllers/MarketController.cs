using MonadNftMarket.Services.Token;
using MonadNftMarket.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MonadNftMarket.Context;

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

        var response = await magicEdenProvider.GetTokensAsync(address);

        if (response.Count > 0)
            return Ok(response);

        return NotFound();
    }

    [HttpGet("market-listing")]
    public async Task<IActionResult> GetMarketListing()
    {
        var listings = await db.Listings.ToListAsync();
        
        return Ok(listings);
    }
}