using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Entities;

public class PasswordResetToken : BaseEntity
{
    public string Token { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime? UsedAt { get; private set; }

    // Navigation property
    public User User { get; private set; } = null!;

    private PasswordResetToken() { } // For EF Core

    private PasswordResetToken(string token, Guid userId, DateTime expiryDate)
    {
        Token = token;
        UserId = userId;
        ExpiryDate = expiryDate;
        IsUsed = false;
    }

    public static PasswordResetToken Create(string token, Guid userId, int expirationHours = 1)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty", nameof(token));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        var expiryDate = DateTime.UtcNow.AddHours(expirationHours);
        return new PasswordResetToken(token, userId, expiryDate);
    }

    public void MarkAsUsed()
    {
        if (IsUsed)
            return;

        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiryDate;

    public bool IsValid => !IsUsed && !IsExpired;
}