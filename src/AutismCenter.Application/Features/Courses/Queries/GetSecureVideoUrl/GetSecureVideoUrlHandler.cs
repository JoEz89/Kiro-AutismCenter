using MediatR;
using Microsoft.Extensions.Logging;
using AutismCenter.Domain.Interfaces;

namespace AutismCenter.Application.Features.Courses.Queries.GetSecureVideoUrl;

public class GetSecureVideoUrlHandler : IRequestHandler<GetSecureVideoUrlQuery, GetSecureVideoUrlResponse>
{
    private readonly IVideoStreamingService _videoStreamingService;
    private readonly IVideoAccessService _videoAccessService;
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger<GetSecureVideoUrlHandler> _logger;

    public GetSecureVideoUrlHandler(
        IVideoStreamingService videoStreamingService,
        IVideoAccessService videoAccessService,
        ICourseRepository courseRepository,
        ILogger<GetSecureVideoUrlHandler> logger)
    {
        _videoStreamingService = videoStreamingService;
        _videoAccessService = videoAccessService;
        _courseRepository = courseRepository;
        _logger = logger;
    }

    public async Task<GetSecureVideoUrlResponse> Handle(GetSecureVideoUrlQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate video access
            var accessResult = await _videoAccessService.ValidateModuleVideoAccessAsync(request.UserId, request.ModuleId);
            if (!accessResult.AccessGranted)
            {
                _logger.LogWarning("Video access denied for user {UserId} and module {ModuleId}: {Reason}", 
                    request.UserId, request.ModuleId, accessResult.Reason);
                throw new UnauthorizedAccessException(accessResult.Reason);
            }

            // Get module details
            var module = await _courseRepository.GetModuleByIdAsync(request.ModuleId, cancellationToken);
            if (module == null)
            {
                throw new InvalidOperationException("Course module not found");
            }

            // Check if user can start a new streaming session
            var canStartSession = await _videoAccessService.CanStartStreamingSessionAsync(request.UserId, request.ModuleId);
            if (!canStartSession)
            {
                throw new InvalidOperationException("Cannot start streaming session. You may already have an active session or have reached the maximum number of concurrent sessions.");
            }

            // Generate session ID
            var sessionId = Guid.NewGuid().ToString();

            // Start streaming session
            await _videoAccessService.StartStreamingSessionAsync(request.UserId, request.ModuleId, sessionId);

            // Extract video key from module's VideoUrl (assuming it's stored as just the key)
            var videoKey = ExtractVideoKeyFromUrl(module.VideoUrl);

            // Generate secure streaming URL
            var streamingUrl = await _videoStreamingService.GenerateSecureStreamingUrlAsync(
                videoKey, request.UserId, request.ExpirationMinutes);

            // Log successful access
            await _videoAccessService.LogVideoAccessAttemptAsync(
                request.UserId, request.ModuleId, true, "Secure streaming URL generated successfully");

            _logger.LogInformation("Generated secure video URL for user {UserId}, module {ModuleId}, session {SessionId}", 
                request.UserId, request.ModuleId, sessionId);

            return new GetSecureVideoUrlResponse
            {
                StreamingUrl = streamingUrl,
                SessionId = sessionId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(request.ExpirationMinutes),
                DaysRemaining = accessResult.DaysRemaining ?? 0,
                ModuleTitle = module.TitleEn, // Could be localized based on user preference
                ModuleDuration = module.DurationInMinutes
            };
        }
        catch (UnauthorizedAccessException)
        {
            // Log failed access attempt
            await _videoAccessService.LogVideoAccessAttemptAsync(
                request.UserId, request.ModuleId, false, "Access denied");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating secure video URL for user {UserId} and module {ModuleId}", 
                request.UserId, request.ModuleId);
            
            // Log failed access attempt
            await _videoAccessService.LogVideoAccessAttemptAsync(
                request.UserId, request.ModuleId, false, $"Error: {ex.Message}");
            throw;
        }
    }

    private static string ExtractVideoKeyFromUrl(string videoUrl)
    {
        // This is a simplified implementation
        // In a real scenario, you might store the video key directly or have a more sophisticated mapping
        if (string.IsNullOrEmpty(videoUrl))
            throw new ArgumentException("Video URL is empty");

        // If the VideoUrl is already just the key, return it
        // If it's a full URL, extract the key from it
        if (videoUrl.Contains("/"))
        {
            return Path.GetFileName(videoUrl);
        }

        return videoUrl;
    }
}