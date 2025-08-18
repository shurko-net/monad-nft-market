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

    public string? GetUserId(HubConnectionContext connection)
    {
        var user = connection.User;
        
        var sub = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!string.IsNullOrEmpty(sub)) return NormalizeAddress(sub);
        
        var address = user.FindFirst("address")?.Value;
        if (!string.IsNullOrEmpty(address)) return NormalizeAddress(address);
        
        var nameId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(nameId)) return NormalizeAddress(nameId);
        
        return null;
    }
    
    private static string NormalizeAddress(string addr) =>
        addr.Trim().ToLowerInvariant();
}