using MonadNftMarket.Services.Token;
using MonadNftMarket.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MonadNftMarket.Controllers;

[ApiController]
[Route("api/market")]
public class MarketController(
    IUserIdentity userIdentity,
    IMagicEdenProvider magicEdenProvider) : ControllerBase
{
    //[Authorize]
    [HttpGet("get-user-tokens")]
    public async Task<IActionResult> GetUserTokens()
    {
        //var address = userIdentity.GetAddressByCookie(HttpContext);

        var address = "0xd365D7486Ba11856bc9b559D0e52829aC0db8745";
        var response = await magicEdenProvider.GetTokensAsync(address);

        if (response.Count > 0)
            return Ok(response);

        return NotFound();
    }
}