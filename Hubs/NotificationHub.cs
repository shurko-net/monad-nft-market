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
    private readonly IUserIdentity _userIdentity;
    public NotificationHub(
        ApiDbContext db,
        ILogger<NotificationHub> logger,
        INotificationService notificationService,
        IUserIdentity userIdentity)
    {
        _db = db;
        _logger = logger;
        _notificationService = notificationService;
        _userIdentity = userIdentity;
    }
    
    public override async Task OnConnectedAsync()
    {
        var userId = _userIdentity.GetAddressByHub(Context.User);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Anonymous connection attempted to NotificationHub. ConnectionId={ConnectionId}", Context.ConnectionId);
            Context.Abort();
            return;
        }
        
        _logger.LogWarning("Successfully connected to NotificationHub");
        
        await Clients.Caller.SendAsync(HubMethods.UnreadCountUpdated,
            await _notificationService.GetUnreadCountAsync(userId));

        var unreaded = await _notificationService.GetUnreadNotifications();

        await Clients.Caller.SendAsync(HubMethods.InitNotifications, unreaded);
        
        await base.OnConnectedAsync();
    }
    public async Task MarkAsRead(Guid notificationId)
    {
        var userId = _userIdentity.GetAddressByHub(Context.User);
        if (string.IsNullOrEmpty(userId)) return;
        
        await _notificationService.MarkAsReadAsync(userId, notificationId);

        var unreaded = await _notificationService.GetUnreadNotifications();

        await Clients.Caller.SendAsync(HubMethods.InitNotifications, unreaded);
        
        await Clients.Caller.SendAsync(HubMethods.NotificationMarkedAsRead, notificationId);
        await Clients.Caller.SendAsync(HubMethods.UnreadCountUpdated,
            await _notificationService.GetUnreadCountAsync(userId));
    }
}