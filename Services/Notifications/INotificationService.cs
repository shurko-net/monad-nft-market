using MonadNftMarket.Models;

namespace MonadNftMarket.Services.Notifications;

public interface INotificationService
{
    Task NotifyAsync(string userAddress, NotificationType type, string title, string body);
    Task MarkAsReadAsync(string userAddress, Guid notificationId);
    Task<List<Notification>> GetUnreadNotifications();
    Task<int> GetUnreadCountAsync(string userAddress);
}