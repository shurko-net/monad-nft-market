using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace MonadNftMarket.Services.Token;

public interface IUserIdentity
{
    string GetAddressByCookie(HttpContext context);
    string GetAddressByHub(ClaimsPrincipal? claims);
}