using Microsoft.Extensions.Logging;
using Moq;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Infrastructure.Services;
using Xunit;

namespace AutismCenter.Infrastructure.Tests.Services;

public class VideoAccessServiceTests
{
    private readonly Mock<IEnrollmentRepository> _mockEnrollmentRepository;
    private readonly Mock<ICourseRepository> _mockCourseRepository;
    private readonly Mock<IVideoStreamingSessionRepository> _mockSessionRepository;
    private readonly Mock<IVideoAccessLogRepository> _mockAccessLogRepository;
    private readonly Mock<ILogger<VideoAccessService>> _mockLogger;
    private readonly VideoAccessService _service;

    public VideoAccessServiceTests()
    {
        _mockEnrollmentRepository = new Mock<IEnrollmentRepository>();
        _mockCourseRepository = new Mock<ICourseRepository>();
        _mockSessionRepository = new Mock<IVideoStreamingSessionRepository>();
        _mockAccessLogRepository = new Mock<IVideoAccessLogRepository>();
        _mockLogger = new Mock<ILogger<VideoAccessService>>();

        _service = new VideoAccessService(
            _mockEnrollmentRepository.Object,
            _mockCourseRepository.Object,
            _mockSessionRepository.Object,
            _mockAccessLogRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ValidateModuleVideoAccessAsync_ShouldReturnGranted_WhenUserHasValidEnrollment()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var module = CourseModule.Create(courseId, "Test Module", "وحدة اختبار", "test-video.mp4", 60, 1);
        var course = Course.Create("Test Course", "دورة اختبار", "Description", "وصف", 120, Money.Create(100, "USD"), "CRS-001");
        var enrollment = Enrollment.CreateEnrollment(userId, courseId, 30);

        _mockCourseRepository.Setup(r => r.GetModuleByIdAsync(moduleId, default))
            .ReturnsAsync(module);
        _mockCourseRepository.Setup(r => r.GetByIdAsync(courseId, default))
            .ReturnsAsync(course);
        _mockEnrollmentRepository.Setup(r => r.GetActiveEnrollmentAsync(userId, courseId, default))
            .ReturnsAsync(enrollment);
        _mockAccessLogRepository.Setup(r => r.CountFailedAccessAttemptsAsync(userId, It.IsAny<TimeSpan>()))
            .ReturnsAsync(0);

        // Act
        var result = await _service.ValidateModuleVideoAccessAsync(userId, moduleId);

        // Assert
        Assert.True(result.AccessGranted);
        Assert.Equal("Access granted", result.Reason);
        Assert.True(result.DaysRemaining > 0);
    }

    [Fact]
    public async Task ValidateModuleVideoAccessAsync_ShouldReturnDenied_WhenModuleNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();

        _mockCourseRepository.Setup(r => r.GetModuleByIdAsync(moduleId, default))
            .ReturnsAsync((CourseModule?)null);

        // Act
        var result = await _service.ValidateModuleVideoAccessAsync(userId, moduleId);

        // Assert
        Assert.False(result.AccessGranted);
        Assert.Equal("Course module not found", result.Reason);
    }

    [Fact]
    public async Task ValidateModuleVideoAccessAsync_ShouldReturnDenied_WhenEnrollmentExpired()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var module = CourseModule.Create(courseId, "Test Module", "وحدة اختبار", "test-video.mp4", 60, 1);
        var course = Course.Create("Test Course", "دورة اختبار", "Description", "وصف", 120, Money.Create(100, "USD"), "CRS-001");
        
        // Create an expired enrollment
        var enrollment = Enrollment.CreateEnrollment(userId, courseId, -1); // Expired yesterday

        _mockCourseRepository.Setup(r => r.GetModuleByIdAsync(moduleId, default))
            .ReturnsAsync(module);
        _mockCourseRepository.Setup(r => r.GetByIdAsync(courseId, default))
            .ReturnsAsync(course);
        _mockEnrollmentRepository.Setup(r => r.GetActiveEnrollmentAsync(userId, courseId, default))
            .ReturnsAsync((Enrollment?)null); // No active enrollment

        // Act
        var result = await _service.ValidateModuleVideoAccessAsync(userId, moduleId);

        // Assert
        Assert.False(result.AccessGranted);
        Assert.Equal("No active enrollment found for this course", result.Reason);
    }

    [Fact]
    public async Task ValidateModuleVideoAccessAsync_ShouldReturnDenied_WhenTooManyFailedAttempts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var module = CourseModule.Create(courseId, "Test Module", "وحدة اختبار", "test-video.mp4", 60, 1);
        var course = Course.Create("Test Course", "دورة اختبار", "Description", "وصف", 120, Money.Create(100, "USD"), "CRS-001");
        var enrollment = Enrollment.CreateEnrollment(userId, courseId, 30);

        _mockCourseRepository.Setup(r => r.GetModuleByIdAsync(moduleId, default))
            .ReturnsAsync(module);
        _mockCourseRepository.Setup(r => r.GetByIdAsync(courseId, default))
            .ReturnsAsync(course);
        _mockEnrollmentRepository.Setup(r => r.GetActiveEnrollmentAsync(userId, courseId, default))
            .ReturnsAsync(enrollment);
        _mockAccessLogRepository.Setup(r => r.CountFailedAccessAttemptsAsync(userId, It.IsAny<TimeSpan>()))
            .ReturnsAsync(15); // Too many failed attempts

        // Act
        var result = await _service.ValidateModuleVideoAccessAsync(userId, moduleId);

        // Assert
        Assert.False(result.AccessGranted);
        Assert.Contains("Too many failed access attempts", result.Reason);
    }

    [Fact]
    public async Task CanStartStreamingSessionAsync_ShouldReturnFalse_WhenUserHasActiveSession()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var existingSession = VideoStreamingSession.StartSession(userId, moduleId, "session-123", "127.0.0.1", "Test Agent");

        _mockSessionRepository.Setup(r => r.GetActiveSessionAsync(userId, moduleId))
            .ReturnsAsync(existingSession);

        // Act
        var result = await _service.CanStartStreamingSessionAsync(userId, moduleId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CanStartStreamingSessionAsync_ShouldReturnFalse_WhenUserHasMaxActiveSessions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();

        _mockSessionRepository.Setup(r => r.GetActiveSessionAsync(userId, moduleId))
            .ReturnsAsync((VideoStreamingSession?)null);
        _mockSessionRepository.Setup(r => r.CountActiveSessionsAsync(userId))
            .ReturnsAsync(2); // More than allowed

        // Act
        var result = await _service.CanStartStreamingSessionAsync(userId, moduleId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CanStartStreamingSessionAsync_ShouldReturnTrue_WhenUserCanStartSession()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();

        _mockSessionRepository.Setup(r => r.GetActiveSessionAsync(userId, moduleId))
            .ReturnsAsync((VideoStreamingSession?)null);
        _mockSessionRepository.Setup(r => r.CountActiveSessionsAsync(userId))
            .ReturnsAsync(0);

        // Act
        var result = await _service.CanStartStreamingSessionAsync(userId, moduleId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task StartStreamingSessionAsync_ShouldCreateNewSession()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var sessionId = "session-123";

        // Act
        await _service.StartStreamingSessionAsync(userId, moduleId, sessionId);

        // Assert
        _mockSessionRepository.Verify(r => r.EndExpiredSessionsAsync(), Times.Once);
        _mockSessionRepository.Verify(r => r.AddAsync(It.Is<VideoStreamingSession>(s =>
            s.UserId == userId &&
            s.ModuleId == moduleId &&
            s.SessionId == sessionId &&
            s.IsActive
        )), Times.Once);
    }

    [Fact]
    public async Task EndStreamingSessionAsync_ShouldEndActiveSession()
    {
        // Arrange
        var sessionId = "session-123";
        var session = VideoStreamingSession.StartSession(Guid.NewGuid(), Guid.NewGuid(), sessionId, "127.0.0.1", "Test Agent");

        _mockSessionRepository.Setup(r => r.GetBySessionIdAsync(sessionId))
            .ReturnsAsync(session);

        // Act
        await _service.EndStreamingSessionAsync(sessionId);

        // Assert
        _mockSessionRepository.Verify(r => r.UpdateAsync(It.Is<VideoStreamingSession>(s =>
            s.SessionId == sessionId &&
            !s.IsActive
        )), Times.Once);
    }

    [Fact]
    public async Task LogVideoAccessAttemptAsync_ShouldCreateAccessLog()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var accessGranted = true;
        var reason = "Access granted";

        // Act
        await _service.LogVideoAccessAttemptAsync(userId, moduleId, accessGranted, reason);

        // Assert
        _mockAccessLogRepository.Verify(r => r.AddAsync(It.Is<VideoAccessLog>(log =>
            log.UserId == userId &&
            log.ModuleId == moduleId &&
            log.AccessGranted == accessGranted &&
            log.Reason == reason
        )), Times.Once);
    }
}