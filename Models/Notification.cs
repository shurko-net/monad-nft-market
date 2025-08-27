using System.ComponentModel.DataAnnotations;

namespace MonadNftMarket.Models;

public class Notification
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    [Required, MaxLength(50)]
    public string UserAddress
    {
        get => _userAddress;
        init => _userAddress = string.IsNullOrEmpty(value) ? string.Empty : NormalizeAddress(value);
    }
    public NotificationType Type { get; init; }
    [MaxLength(200)] public string Title { get; init; } = string.Empty;
    [MaxLength(300)] public string Body { get; init; } = string.Empty;
    public bool IsRead { get; set; }
    [Required, MaxLength(70)]
    public string TransactionHash
    {
        get => _transactionHash;
        init => _transactionHash = string.IsNullOrEmpty(value) ? string.Empty : NormalizeAddress(value);
    }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    
    private readonly string _userAddress = string.Empty;
    private readonly string _transactionHash = string.Empty;
    private static string NormalizeAddress(string? addr) =>
        string.IsNullOrWhiteSpace(addr) ? string.Empty : addr.Trim().ToLowerInvariant();
}