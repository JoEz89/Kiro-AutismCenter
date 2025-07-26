using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Entities;

public class VideoAccessLog : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid ModuleId { get; private set; }
    public DateTime AccessTime { get; private set; }
    public bool AccessGranted { get; private set; }
    public string Reason { get; private set; }
    public string IpAddress { get; private set; }
    public string UserAgent { get; private set; }
    public string? SessionId { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public CourseModule Module { get; private set; } = null!;

    private VideoAccessLog() { } // For EF Core

    private VideoAccessLog(Guid userId, Guid moduleId, bool accessGranted, string reason, 
                          string ipAddress, string userAgent, string? sessionId = null)
    {
        UserId = userId;
        ModuleId = moduleId;
        AccessTime = DateTime.UtcNow;
        AccessGranted = accessGranted;
        Reason = reason;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        SessionId = sessionId;
    }

    public static VideoAccessLog CreateLog(Guid userId, Guid moduleId, bool accessGranted, string reason,
                                          string ipAddress, string userAgent, string? sessionId = null)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason cannot be empty", nameof(reason));

        if (string.IsNullOrWhiteSpace(ipAddress))
            throw new ArgumentException("IP address cannot be empty", nameof(ipAddress));

        if (string.IsNullOrWhiteSpace(userAgent))
            throw new ArgumentException("User agent cannot be empty", nameof(userAgent));

        return new VideoAccessLog(userId, moduleId, accessGranted, reason, ipAddress, userAgent, sessionId);
    }
}