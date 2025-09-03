using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MonadNftMarket.Context;
using MonadNftMarket.Hubs;
using MonadNftMarket.Models;

namespace MonadNftMarket.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hub;
    private readonly ILogger<NotificationService> _logger;
    private readonly IDbContextFactory<ApiDbContext> _db;

    public NotificationService(
        IDbContextFactory<ApiDbContext> db,
        IHubContext<NotificationHub> hub,
        ILogger<NotificationService> logger)
    {
        _db = db;
        _hub = hub;
        _logger = logger;
    }

    private static string Normalize(string address) => address.Trim().ToLowerInvariant();

    public async Task NotifyAsync(
        string userAddress,
        EventStatus status,
        string title,
        string body)
    {
        await using var db = await _db.CreateDbContextAsync();
        userAddress = Normalize(userAddress);

        var notification = new Notification
        {
            UserAddress = userAddress,
            Status = status,
            Title = title,
            Body = body,
            IsRead = false
        };

        db.Notifications.Add(notification);
        await db.SaveChangesAsync();

        try
        {
            await _hub.Clients.User(userAddress)
                .SendAsync(HubMethods.UnreadCountUpdated, await GetUnreadCountAsync(userAddress));
            await _hub.Clients.User(userAddress).SendAsync(HubMethods.InitNotifications,
                await GetUnreadNotificationsAsync(userAddress));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to push SignalR notification to {User}", userAddress);
        }
    }

    public async Task MarkAsReadAsync(string userAddress, Guid notificationId)
    {
        await using var db = await _db.CreateDbContextAsync();

        var n = await db.Notifications
            .FirstOrDefaultAsync(n => n.UserAddress == userAddress && n.Id == notificationId);
        if (n == null) return;
        n.IsRead = true;

        await db.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(string userAddress)
    {
        await using var db = await _db.CreateDbContextAsync();

        await db.Notifications
            .Where(n => n.UserAddress == userAddress && !n.IsRead)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(n => n.IsRead, true));
    }

    public async Task<int> GetUnreadCountAsync(string userAddress)
    {
        await using var db = await _db.CreateDbContextAsync();
        
        return await db.Notifications.CountAsync(n => n.UserAddress == userAddress && !n.IsRead);
    }

    public async Task NotifyMarketUpdateAsync()
        => await _hub.Clients.All.SendAsync(HubMethods.UpdateMarket, "Market updated");

    public async Task<List<Notification>> GetUnreadNotificationsAsync(string userAddress)
    {
        await using var db = await _db.CreateDbContextAsync();
        
        var notifications = await db.Notifications
            .Where(n => !n.IsRead && n.UserAddress == userAddress)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new Notification
            {
                Id = n.Id,
                UserAddress = n.UserAddress,
                Status = n.Status,
                Title = n.Title,
                Body = n.Body,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();

        return notifications;
    }
}