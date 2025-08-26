using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.SignalR;

namespace MonadNftMarket.Services.Token; 

public class WalletUserIdProvider : IUserIdProvider
{
    
    public string? GetUserId(HubConnectionContext connection)
    {
        var sub = connection.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if(!string.IsNullOrEmpty(sub))
            return sub.Trim().ToLowerInvariant();
        
        var addr = connection.User?.FindFirst("address")?.Value;
        return string.IsNullOrEmpty(addr) ? null : addr.Trim().ToLowerInvariant();
    }
}