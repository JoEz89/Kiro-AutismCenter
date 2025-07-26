using Amazon.S3;
using Amazon.S3.Model;
using AutismCenter.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AutismCenter.Infrastructure.Services;

public class AwsS3VideoStreamingService : IVideoStreamingService
{
    private readonly IAmazonS3 _s3Client;
    private readonly IVideoAccessService _videoAccessService;
    private readonly ILogger<AwsS3VideoStreamingService> _logger;
    private readonly string _bucketName;
    private readonly string _videoPrefix;

    public AwsS3VideoStreamingService(
        IAmazonS3 s3Client,
        IVideoAccessService videoAccessService,
        IConfiguration configuration,
        ILogger<AwsS3VideoStreamingService> logger)
    {
        _s3Client = s3Client;
        _videoAccessService = videoAccessService;
        _logger = logger;
        _bucketName = configuration["AWS:S3:VideoBucketName"] ?? throw new InvalidOperationException("AWS S3 video bucket name not configured");
        _videoPrefix = configuration["AWS:S3:VideoPrefix"] ?? "course-videos/";
    }

    public async Task<string> GenerateSecureStreamingUrlAsync(string videoKey, Guid userId, int expirationMinutes = 60)
    {
        try
        {
            // Note: Access validation is handled at the application layer before calling this method
            // This service focuses on generating the secure URL

            // Generate pre-signed URL with custom headers to prevent downloads
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = $"{_videoPrefix}{videoKey}",
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                ResponseHeaderOverrides = new ResponseHeaderOverrides
                {
                    ContentDisposition = "inline", // Prevent download, force inline viewing
                    CacheControl = "no-cache, no-store, must-revalidate", // Prevent caching
                    ContentType = "video/mp4" // Ensure proper content type
                }
            };

            // Add custom headers to prevent downloads and enhance security
            request.Headers.Add("X-User-ID", userId.ToString());
            request.Headers.Add("X-Timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

            var presignedUrl = await _s3Client.GetPreSignedURLAsync(request);

            _logger.LogInformation("Generated secure streaming URL for user {UserId} and video {VideoKey}, expires in {ExpirationMinutes} minutes", 
                userId, videoKey, expirationMinutes);

            return presignedUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate secure streaming URL for user {UserId} and video {VideoKey}", userId, videoKey);
            throw;
        }
    }

    public async Task<bool> ValidateVideoAccessAsync(string videoKey, Guid userId)
    {
        try
        {
            // This method is not used in our current implementation
            // Access validation is handled at the application layer
            _logger.LogWarning("ValidateVideoAccessAsync called but not implemented for video key validation");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate video access for user {UserId} and video {VideoKey}", userId, videoKey);
            return false;
        }
    }

    public async Task<string> UploadVideoAsync(Stream videoStream, string fileName, string contentType)
    {
        try
        {
            var videoKey = GenerateVideoKey(fileName);
            var fullKey = $"{_videoPrefix}{videoKey}";

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = fullKey,
                InputStream = videoStream,
                ContentType = contentType,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256,
                Metadata =
                {
                    ["original-filename"] = fileName,
                    ["upload-timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ["content-type"] = contentType
                }
            };

            // Set bucket policy to prevent direct access
            request.Headers.Add("x-amz-acl", "private");

            var response = await _s3Client.PutObjectAsync(request);

            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation("Successfully uploaded video {VideoKey} with original filename {FileName}", videoKey, fileName);
                return videoKey;
            }

            throw new InvalidOperationException($"Failed to upload video. HTTP Status: {response.HttpStatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload video {FileName}", fileName);
            throw;
        }
    }

    public async Task DeleteVideoAsync(string videoKey)
    {
        try
        {
            var fullKey = $"{_videoPrefix}{videoKey}";
            
            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = fullKey
            };

            var response = await _s3Client.DeleteObjectAsync(request);
            
            _logger.LogInformation("Successfully deleted video {VideoKey}", videoKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete video {VideoKey}", videoKey);
            throw;
        }
    }

    public async Task<VideoMetadata> GetVideoMetadataAsync(string videoKey)
    {
        try
        {
            var fullKey = $"{_videoPrefix}{videoKey}";
            
            var request = new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = fullKey
            };

            var response = await _s3Client.GetObjectMetadataAsync(request);

            return new VideoMetadata
            {
                VideoKey = videoKey,
                FileName = response.Metadata.ContainsKey("original-filename") ? response.Metadata["original-filename"] : videoKey,
                ContentType = response.Headers.ContentType,
                FileSizeBytes = response.ContentLength,
                UploadedAt = response.LastModified,
                ThumbnailUrl = null // Could be implemented later
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get video metadata for {VideoKey}", videoKey);
            throw;
        }
    }

    private static string GenerateVideoKey(string fileName)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var guid = Guid.NewGuid().ToString("N")[..8]; // First 8 characters of GUID
        var extension = Path.GetExtension(fileName);
        return $"{timestamp}_{guid}{extension}";
    }


}