using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public bool IsRevoked { get; private set; }
    public string? RevokedReason { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    // Navigation property
    public User User { get; private set; } = null!;

    private RefreshToken() { } // For EF Core

    private RefreshToken(string token, Guid userId, DateTime expiryDate)
    {
        Token = token;
        UserId = userId;
        ExpiryDate = expiryDate;
        IsRevoked = false;
    }

    public static RefreshToken Create(string token, Guid userId, DateTime expiryDate)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty", nameof(token));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (expiryDate <= DateTime.UtcNow)
            throw new ArgumentException("Expiry date must be in the future", nameof(expiryDate));

        return new RefreshToken(token, userId, expiryDate);
    }

    public void Revoke(string reason = "Manual revocation")
    {
        if (IsRevoked)
            return;

        IsRevoked = true;
        RevokedReason = reason;
        RevokedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiryDate;

    public bool IsActive => !IsRevoked && !IsExpired;
}