using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Entities;

public class VideoStreamingSession : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid ModuleId { get; private set; }
    public string SessionId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public string IpAddress { get; private set; }
    public string UserAgent { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public CourseModule Module { get; private set; } = null!;

    private VideoStreamingSession() { } // For EF Core

    private VideoStreamingSession(Guid userId, Guid moduleId, string sessionId, string ipAddress, string userAgent)
    {
        UserId = userId;
        ModuleId = moduleId;
        SessionId = sessionId;
        StartTime = DateTime.UtcNow;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        IsActive = true;
    }

    public static VideoStreamingSession StartSession(Guid userId, Guid moduleId, string sessionId, string ipAddress, string userAgent)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID cannot be empty", nameof(sessionId));

        if (string.IsNullOrWhiteSpace(ipAddress))
            throw new ArgumentException("IP address cannot be empty", nameof(ipAddress));

        if (string.IsNullOrWhiteSpace(userAgent))
            throw new ArgumentException("User agent cannot be empty", nameof(userAgent));

        return new VideoStreamingSession(userId, moduleId, sessionId, ipAddress, userAgent);
    }

    public void EndSession()
    {
        if (!IsActive)
            return;

        EndTime = DateTime.UtcNow;
        IsActive = false;
        UpdateTimestamp();
    }

    public TimeSpan GetSessionDuration()
    {
        var endTime = EndTime ?? DateTime.UtcNow;
        return endTime - StartTime;
    }

    public bool IsExpired(int maxDurationMinutes = 120) // 2 hours default
    {
        return DateTime.UtcNow > StartTime.AddMinutes(maxDurationMinutes);
    }
}