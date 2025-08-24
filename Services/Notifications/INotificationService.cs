using MonadNftMarket.Models;

namespace MonadNftMarket.Services.Notifications;

public interface INotificationService
{
    Task NotifyAsync(string userAddress, NotificationType type, string title, string body);
    Task MarkAsReadAsync(string userAddress, Guid notificationId);
    Task<List<Notification>> GetUnreadNotifications(string userAddress);
    Task<int> GetUnreadCountAsync(string userAddress);
}