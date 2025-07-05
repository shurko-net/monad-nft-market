namespace MonadNftMarket.Services.Token;

public interface IUserIdentity
{
    string GetAddressByCookie(HttpContext context);
}