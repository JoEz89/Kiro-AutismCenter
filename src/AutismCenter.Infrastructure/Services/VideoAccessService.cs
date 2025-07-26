using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AutismCenter.Infrastructure.Services;

public class VideoAccessService : IVideoAccessService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IVideoStreamingSessionRepository _sessionRepository;
    private readonly IVideoAccessLogRepository _accessLogRepository;
    private readonly ILogger<VideoAccessService> _logger;

    public VideoAccessService(
        IEnrollmentRepository enrollmentRepository,
        ICourseRepository courseRepository,
        IVideoStreamingSessionRepository sessionRepository,
        IVideoAccessLogRepository accessLogRepository,
        ILogger<VideoAccessService> logger)
    {
        _enrollmentRepository = enrollmentRepository;
        _courseRepository = courseRepository;
        _sessionRepository = sessionRepository;
        _accessLogRepository = accessLogRepository;
        _logger = logger;
    }

    public async Task<VideoAccessResult> ValidateModuleVideoAccessAsync(Guid userId, Guid moduleId)
    {
        try
        {
            // Get the course module to find the associated course
            var module = await _courseRepository.GetModuleByIdAsync(moduleId);
            if (module == null)
            {
                return VideoAccessResult.Denied("Course module not found");
            }

            if (!module.IsActive)
            {
                return VideoAccessResult.Denied("Course module is not active");
            }

            // Check if the course is active
            var course = await _courseRepository.GetByIdAsync(module.CourseId);
            if (course == null || !course.IsActive)
            {
                return VideoAccessResult.Denied("Course is not active");
            }

            // Check if user has an active enrollment for this course
            var enrollment = await _enrollmentRepository.GetActiveEnrollmentAsync(userId, module.CourseId);
            if (enrollment == null)
            {
                return VideoAccessResult.Denied("No active enrollment found for this course");
            }

            // Check if enrollment is expired
            if (enrollment.IsExpired())
            {
                return VideoAccessResult.Denied("Course enrollment has expired");
            }

            // Check if enrollment is active
            if (!enrollment.CanAccess())
            {
                return VideoAccessResult.Denied("Enrollment is not active or has expired");
            }

            // Check for suspicious activity (too many failed attempts recently)
            var recentFailedAttempts = await _accessLogRepository.CountFailedAccessAttemptsAsync(userId, TimeSpan.FromHours(1));
            if (recentFailedAttempts > 10) // Configurable threshold
            {
                return VideoAccessResult.Denied("Too many failed access attempts. Please try again later.");
            }

            // All checks passed
            return VideoAccessResult.Granted(enrollment.ExpiryDate, enrollment.GetDaysRemaining());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating video access for user {UserId} and module {ModuleId}", userId, moduleId);
            return VideoAccessResult.Denied("Internal error occurred while validating access");
        }
    }

    public async Task LogVideoAccessAttemptAsync(Guid userId, Guid moduleId, bool accessGranted, string reason)
    {
        try
        {
            // In a real implementation, you'd get these from the HTTP context
            var ipAddress = "127.0.0.1"; // Placeholder
            var userAgent = "Unknown"; // Placeholder

            var log = VideoAccessLog.CreateLog(userId, moduleId, accessGranted, reason, ipAddress, userAgent);
            await _accessLogRepository.AddAsync(log);

            _logger.LogInformation("Logged video access attempt for user {UserId}, module {ModuleId}, granted: {AccessGranted}, reason: {Reason}",
                userId, moduleId, accessGranted, reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log video access attempt for user {UserId} and module {ModuleId}", userId, moduleId);
            // Don't throw here as logging failures shouldn't break the main flow
        }
    }

    public async Task<bool> CanStartStreamingSessionAsync(Guid userId, Guid moduleId)
    {
        try
        {
            // Check if user already has an active session for this module
            var existingSession = await _sessionRepository.GetActiveSessionAsync(userId, moduleId);
            if (existingSession != null && !existingSession.IsExpired())
            {
                _logger.LogWarning("User {UserId} already has an active streaming session for module {ModuleId}", userId, moduleId);
                return false;
            }

            // Check total number of active sessions for the user (prevent multiple device access)
            var activeSessionCount = await _sessionRepository.CountActiveSessionsAsync(userId);
            if (activeSessionCount >= 1) // Only allow one active session per user
            {
                _logger.LogWarning("User {UserId} already has {ActiveSessionCount} active streaming sessions", userId, activeSessionCount);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} can start streaming session for module {ModuleId}", userId, moduleId);
            return false;
        }
    }

    public async Task StartStreamingSessionAsync(Guid userId, Guid moduleId, string sessionId)
    {
        try
        {
            // End any expired sessions first
            await _sessionRepository.EndExpiredSessionsAsync();

            // In a real implementation, you'd get these from the HTTP context
            var ipAddress = "127.0.0.1"; // Placeholder
            var userAgent = "Unknown"; // Placeholder

            var session = VideoStreamingSession.StartSession(userId, moduleId, sessionId, ipAddress, userAgent);
            await _sessionRepository.AddAsync(session);

            _logger.LogInformation("Started streaming session {SessionId} for user {UserId} and module {ModuleId}", 
                sessionId, userId, moduleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start streaming session for user {UserId} and module {ModuleId}", userId, moduleId);
            throw;
        }
    }

    public async Task EndStreamingSessionAsync(string sessionId)
    {
        try
        {
            var session = await _sessionRepository.GetBySessionIdAsync(sessionId);
            if (session != null && session.IsActive)
            {
                session.EndSession();
                await _sessionRepository.UpdateAsync(session);

                _logger.LogInformation("Ended streaming session {SessionId}", sessionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to end streaming session {SessionId}", sessionId);
            throw;
        }
    }
}