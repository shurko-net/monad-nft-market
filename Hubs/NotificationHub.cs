using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MonadNftMarket.Services.Notifications;
using MonadNftMarket.Services.Token;

namespace MonadNftMarket.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;
    private readonly INotificationService _notificationService;
    private readonly IUserIdentity _userIdentity;
    public NotificationHub(ILogger<NotificationHub> logger,
        INotificationService notificationService,
        IUserIdentity userIdentity)
    {
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
        
        _logger.LogInformation("Successfully connected to NotificationHub");
        
        await Clients.Caller.SendAsync(HubMethods.UnreadCountUpdated,
            await _notificationService.GetUnreadCountAsync(userId));

        var unreaded = await _notificationService.GetUnreadNotificationsAsync(userId);

        await Clients.Caller.SendAsync(HubMethods.InitNotifications, unreaded);
        
        await base.OnConnectedAsync();
    }
    public async Task MarkAsRead(Guid notificationId)
    {
        var userId = _userIdentity.GetAddressByHub(Context.User);
        if (string.IsNullOrEmpty(userId)) return;
        
        await _notificationService.MarkAsReadAsync(userId, notificationId);

        var unreaded = await _notificationService.GetUnreadNotificationsAsync(userId);

        await Clients.Caller.SendAsync(HubMethods.InitNotifications, unreaded);
        
        await Clients.Caller.SendAsync(HubMethods.NotificationMarkedAsRead, notificationId);
        await Clients.Caller.SendAsync(HubMethods.UnreadCountUpdated,
            await _notificationService.GetUnreadCountAsync(userId));
    }

    public async Task MarkAllAsRead()
    {
        var userId = _userIdentity.GetAddressByHub(Context.User);
        if (string.IsNullOrEmpty(userId)) return;
        
        await _notificationService.MarkAllAsReadAsync(userId);
        
        await Clients.Caller.SendAsync(HubMethods.UnreadCountUpdated,
            await _notificationService.GetUnreadCountAsync(userId));
    }
}