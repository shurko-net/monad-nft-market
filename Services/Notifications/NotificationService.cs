using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MonadNftMarket.Context;
using MonadNftMarket.Hubs;
using MonadNftMarket.Models;

namespace MonadNftMarket.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly ApiDbContext _db;
    private readonly IHubContext<NotificationHub> _hub;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        ApiDbContext db,
        IHubContext<NotificationHub> hub,
        ILogger<NotificationService> logger)
    {
        _db = db;
        _hub = hub;
        _logger = logger;
    }

    private static string Normalize(string address) => address.Trim().ToLowerInvariant();
    
    public async Task NotifyAsync(string userAddress, NotificationType type, string title, string body)
    {
        userAddress = Normalize(userAddress);
        
        var n = new Notification
        {
            UserAddress = userAddress,
            Type = type,
            Title = title,
            Body = body,
            IsRead = false
        };

        _db.Notifications.Add(n);
        await _db.SaveChangesAsync();

        try
        {
            await _hub.Clients.Users(userAddress).SendAsync(HubMethods.NotificationReceived, new
            {
                id = n.Id, title = n.Title, body = n.Body, created = n.CreatedAt, type = n.Type, isRead = n.IsRead
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to push SignalR notification to {User}", userAddress);
        }
    }

    public async Task MarkAsReadAsync(string userAddress, Guid notificationId)
    {
        var n = await _db.Notifications
            .FirstOrDefaultAsync(n => n.UserAddress == userAddress && n.Id == notificationId);
        if (n == null) return;
        n.IsRead = true;
        
        await _db.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync(string userAddress) 
        => await _db.Notifications.CountAsync(n => n.UserAddress == userAddress && !n.IsRead);
    
}