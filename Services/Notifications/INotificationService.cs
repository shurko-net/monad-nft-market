using MonadNftMarket.Models;

namespace MonadNftMarket.Services.Notifications;

public interface INotificationService
{
    Task NotifyAsync(string userAddress, EventStatus status, string title, string body);
    Task MarkAsReadAsync(string userAddress, Guid notificationId);
    Task MarkAllAsReadAsync(string userAddress);
    Task<List<Notification>> GetUnreadNotificationsAsync(string userAddress);
    Task<int> GetUnreadCountAsync(string userAddress);
    Task NotifyMarketUpdateAsync();
}