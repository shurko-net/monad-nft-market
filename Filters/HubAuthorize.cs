using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using MonadNftMarket.Configuration;

namespace MonadNftMarket.Filters;

public class HubAuthorize(ILogger<HubAuthorize> logger) : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        logger.LogCritical("In filter");
        var httpContext = invocationContext.Context.GetHttpContext() ?? throw new HubException("In filter");

        bool isAuthenticated = httpContext.User.Identity?.IsAuthenticated ?? false;
        
        if (!isAuthenticated)
        {
            logger.LogCritical("User is not authenticated");
            throw new HubException("User is not authenticated");
        }
        
        return await next(invocationContext);
    }
}