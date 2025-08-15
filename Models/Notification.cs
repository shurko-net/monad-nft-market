namespace MonadNftMarket.Models;

public class Notification
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string UserAddress { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}