using AutismCenter.Domain.Entities;

namespace AutismCenter.Domain.Interfaces;

public interface IVideoStreamingService
{
    /// <summary>
    /// Generates a secure, time-limited URL for video streaming
    /// </summary>
    /// <param name="videoKey">The unique identifier/key for the video in storage</param>
    /// <param name="userId">The ID of the user requesting access</param>
    /// <param name="expirationMinutes">How long the URL should remain valid (default: 60 minutes)</param>
    /// <returns>A secure streaming URL that expires after the specified time</returns>
    Task<string> GenerateSecureStreamingUrlAsync(string videoKey, Guid userId, int expirationMinutes = 60);

    /// <summary>
    /// Validates if a user has access to a specific video
    /// </summary>
    /// <param name="videoKey">The unique identifier/key for the video</param>
    /// <param name="userId">The ID of the user requesting access</param>
    /// <returns>True if the user has valid access, false otherwise</returns>
    Task<bool> ValidateVideoAccessAsync(string videoKey, Guid userId);

    /// <summary>
    /// Uploads a video file to secure storage
    /// </summary>
    /// <param name="videoStream">The video file stream</param>
    /// <param name="fileName">The original filename</param>
    /// <param name="contentType">The MIME type of the video</param>
    /// <returns>The unique key/identifier for the uploaded video</returns>
    Task<string> UploadVideoAsync(Stream videoStream, string fileName, string contentType);

    /// <summary>
    /// Deletes a video from storage
    /// </summary>
    /// <param name="videoKey">The unique identifier/key for the video to delete</param>
    Task DeleteVideoAsync(string videoKey);

    /// <summary>
    /// Gets video metadata (duration, size, etc.) without providing access to the actual video
    /// </summary>
    /// <param name="videoKey">The unique identifier/key for the video</param>
    /// <returns>Video metadata information</returns>
    Task<VideoMetadata> GetVideoMetadataAsync(string videoKey);
}

public class VideoMetadata
{
    public string VideoKey { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
    public string? ThumbnailUrl { get; set; }
}