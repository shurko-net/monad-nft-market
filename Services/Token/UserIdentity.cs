using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using MonadNftMarket.Configuration;

namespace MonadNftMarket.Services.Token;

public class UserIdentity(IOptions<EnvVariables> env) : IUserIdentity
{
    private readonly EnvVariables _env = env.Value;
    public string GetAddressByCookie(HttpContext context)
    {
        var cookie = context.Request.Cookies;

        if (!cookie.TryGetValue(_env.CookieName, out var address))
            return string.Empty;

        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(address);

            return jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value.ToLowerInvariant()
                   ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public string GetAddressByHub(ClaimsPrincipal? claims)
    {
        if (claims == null)
            return string.Empty;
        
        var sub = claims.Claims
            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value.ToLowerInvariant();

        if (!string.IsNullOrEmpty(sub))
            return sub;
        
        var address = claims.Claims
            .FirstOrDefault(c => c.Type == "address")?.Value.ToLowerInvariant();
        
        return !string.IsNullOrEmpty(address) ? address : string.Empty;
    }
}