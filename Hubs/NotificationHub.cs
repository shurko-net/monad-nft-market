using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MonadNftMarket.Context;
using MonadNftMarket.Services.Notifications;
using MonadNftMarket.Services.Token;

namespace MonadNftMarket.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ApiDbContext _db;
    private readonly ILogger<NotificationHub> _logger;
    private readonly INotificationService _notificationService;
    private readonly IUserIdentity _identity;
    
    public NotificationHub(
        ApiDbContext db,
        ILogger<NotificationHub> logger,
        INotificationService notificationService,
        IUserIdentity identity)
    {
        _db = db;
        _logger = logger;
        _notificationService = notificationService;
        _identity = identity;
    }
    
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Anonymous connection attempted to NotificationHub. ConnectionId={ConnectionId}", Context.ConnectionId);
            Context.Abort();
            return;
        }
        
        userId = userId.ToLowerInvariant();
        
        await Clients.Caller.SendAsync(HubMethods.UnreadCountUpdated,
            await _notificationService.GetUnreadCountAsync(userId));
        
        var recent = await _db.Notifications
            .Where(n => n.UserAddress == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new
            {
                n.Id, n.Title, n.Body, n.Type, n.IsRead, n.CreatedAt
            })
            .ToListAsync();

        await Clients.Caller.SendAsync(HubMethods.InitNotifications, recent);
        
        await base.OnConnectedAsync();
    }

    public async Task MarkAsRead(Guid notificationId)
    {
        var userId = Context.UserIdentifier;
        if (userId == null) return;
        
        await _notificationService.MarkAsReadAsync(userId, notificationId);
        
        await Clients.Caller.SendAsync(HubMethods.NotificationMarkedAsRead, notificationId);
        await Clients.Caller.SendAsync(HubMethods.UnreadCountUpdated,
            await _notificationService.GetUnreadCountAsync(userId));
    }
}