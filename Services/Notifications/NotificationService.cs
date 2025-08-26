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
        
        var notification = new Notification
        {
            UserAddress = userAddress,
            Type = type,
            Title = title,
            Body = body,
            IsRead = false
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();

        try
        {
            await _hub.Clients.User(userAddress).SendAsync(HubMethods.InitNotifications, new[] { notification });
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

    public async Task MarkAllAsReadAsync(string userAddress)
        => await _db.Notifications
            .Where(n => n.UserAddress == userAddress && !n.IsRead)
            .ExecuteUpdateAsync(s => 
                s.SetProperty(n => n.IsRead, true));

    public async Task<int> GetUnreadCountAsync(string userAddress) 
        => await _db.Notifications.CountAsync(n => n.UserAddress == userAddress && !n.IsRead);

    public async Task<List<Notification>> GetUnreadNotifications(string userAddress)
    {
        var notifications = await _db.Notifications
            .Where(n => !n.IsRead && n.UserAddress == userAddress)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new Notification
            {
                Id = n.Id,
                UserAddress = n.UserAddress,
                Type = n.Type,
                Title = n.Title,
                Body = n.Body,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();

        return notifications;
    }
}