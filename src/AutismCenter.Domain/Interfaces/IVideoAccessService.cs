namespace AutismCenter.Domain.Interfaces;

public interface IVideoAccessService
{
    /// <summary>
    /// Validates if a user can access a specific course module video
    /// </summary>
    /// <param name="userId">The ID of the user requesting access</param>
    /// <param name="moduleId">The ID of the course module containing the video</param>
    /// <returns>VideoAccessResult indicating whether access is granted and why</returns>
    Task<VideoAccessResult> ValidateModuleVideoAccessAsync(Guid userId, Guid moduleId);

    /// <summary>
    /// Records a video access attempt for audit purposes
    /// </summary>
    /// <param name="userId">The ID of the user accessing the video</param>
    /// <param name="moduleId">The ID of the course module</param>
    /// <param name="accessGranted">Whether access was granted</param>
    /// <param name="reason">The reason for the access decision</param>
    Task LogVideoAccessAttemptAsync(Guid userId, Guid moduleId, bool accessGranted, string reason);

    /// <summary>
    /// Checks if a user has an active session for video streaming (prevents multiple device access)
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="moduleId">The ID of the course module</param>
    /// <returns>True if the user can start a new streaming session</returns>
    Task<bool> CanStartStreamingSessionAsync(Guid userId, Guid moduleId);

    /// <summary>
    /// Starts a new streaming session for a user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="moduleId">The ID of the course module</param>
    /// <param name="sessionId">Unique identifier for this streaming session</param>
    Task StartStreamingSessionAsync(Guid userId, Guid moduleId, string sessionId);

    /// <summary>
    /// Ends a streaming session
    /// </summary>
    /// <param name="sessionId">The unique identifier for the streaming session</param>
    Task EndStreamingSessionAsync(string sessionId);
}

public class VideoAccessResult
{
    public bool AccessGranted { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime? EnrollmentExpiry { get; set; }
    public int? DaysRemaining { get; set; }

    public static VideoAccessResult Granted(DateTime? enrollmentExpiry = null, int? daysRemaining = null)
    {
        return new VideoAccessResult
        {
            AccessGranted = true,
            Reason = "Access granted",
            EnrollmentExpiry = enrollmentExpiry,
            DaysRemaining = daysRemaining
        };
    }

    public static VideoAccessResult Denied(string reason)
    {
        return new VideoAccessResult
        {
            AccessGranted = false,
            Reason = reason
        };
    }
}