using Microsoft.Extensions.Logging;
using Moq;
using AutismCenter.Application.Features.Courses.Queries.GetSecureVideoUrl;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using Xunit;

namespace AutismCenter.Application.Tests.Features.Courses;

public class GetSecureVideoUrlHandlerTests
{
    private readonly Mock<IVideoStreamingService> _mockVideoStreamingService;
    private readonly Mock<IVideoAccessService> _mockVideoAccessService;
    private readonly Mock<ICourseRepository> _mockCourseRepository;
    private readonly Mock<ILogger<GetSecureVideoUrlHandler>> _mockLogger;
    private readonly GetSecureVideoUrlHandler _handler;

    public GetSecureVideoUrlHandlerTests()
    {
        _mockVideoStreamingService = new Mock<IVideoStreamingService>();
        _mockVideoAccessService = new Mock<IVideoAccessService>();
        _mockCourseRepository = new Mock<ICourseRepository>();
        _mockLogger = new Mock<ILogger<GetSecureVideoUrlHandler>>();

        _handler = new GetSecureVideoUrlHandler(
            _mockVideoStreamingService.Object,
            _mockVideoAccessService.Object,
            _mockCourseRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSecureUrl_WhenUserHasAccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var query = new GetSecureVideoUrlQuery(userId, moduleId, 60);

        var accessResult = VideoAccessResult.Granted(DateTime.UtcNow.AddDays(15), 15);
        var module = CourseModule.Create(courseId, "Test Module", "وحدة اختبار", "test-video.mp4", 60, 1);
        var sessionId = Guid.NewGuid().ToString();
        var streamingUrl = "https://secure-video-url.com";

        _mockVideoAccessService.Setup(s => s.ValidateModuleVideoAccessAsync(userId, moduleId))
            .ReturnsAsync(accessResult);
        _mockCourseRepository.Setup(r => r.GetModuleByIdAsync(moduleId, default))
            .ReturnsAsync(module);
        _mockVideoAccessService.Setup(s => s.CanStartStreamingSessionAsync(userId, moduleId))
            .ReturnsAsync(true);
        _mockVideoStreamingService.Setup(s => s.GenerateSecureStreamingUrlAsync("test-video.mp4", userId, 60))
            .ReturnsAsync(streamingUrl);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(streamingUrl, result.StreamingUrl);
        Assert.Equal(15, result.DaysRemaining);
        Assert.Equal("Test Module", result.ModuleTitle);
        Assert.Equal(60, result.ModuleDuration);
        Assert.NotEmpty(result.SessionId);

        _mockVideoAccessService.Verify(s => s.StartStreamingSessionAsync(userId, moduleId, It.IsAny<string>()), Times.Once);
        _mockVideoAccessService.Verify(s => s.LogVideoAccessAttemptAsync(userId, moduleId, true, "Secure streaming URL generated successfully"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedException_WhenAccessDenied()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var query = new GetSecureVideoUrlQuery(userId, moduleId, 60);

        var accessResult = VideoAccessResult.Denied("Enrollment expired");

        _mockVideoAccessService.Setup(s => s.ValidateModuleVideoAccessAsync(userId, moduleId))
            .ReturnsAsync(accessResult);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(query, CancellationToken.None));

        Assert.Equal("Enrollment expired", exception.Message);
        _mockVideoAccessService.Verify(s => s.LogVideoAccessAttemptAsync(userId, moduleId, false, "Access denied"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenModuleNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var query = new GetSecureVideoUrlQuery(userId, moduleId, 60);

        var accessResult = VideoAccessResult.Granted(DateTime.UtcNow.AddDays(15), 15);

        _mockVideoAccessService.Setup(s => s.ValidateModuleVideoAccessAsync(userId, moduleId))
            .ReturnsAsync(accessResult);
        _mockCourseRepository.Setup(r => r.GetModuleByIdAsync(moduleId, default))
            .ReturnsAsync((CourseModule?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(query, CancellationToken.None));

        Assert.Equal("Course module not found", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenCannotStartSession()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var query = new GetSecureVideoUrlQuery(userId, moduleId, 60);

        var accessResult = VideoAccessResult.Granted(DateTime.UtcNow.AddDays(15), 15);
        var module = CourseModule.Create(courseId, "Test Module", "وحدة اختبار", "test-video.mp4", 60, 1);

        _mockVideoAccessService.Setup(s => s.ValidateModuleVideoAccessAsync(userId, moduleId))
            .ReturnsAsync(accessResult);
        _mockCourseRepository.Setup(r => r.GetModuleByIdAsync(moduleId, default))
            .ReturnsAsync(module);
        _mockVideoAccessService.Setup(s => s.CanStartStreamingSessionAsync(userId, moduleId))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(query, CancellationToken.None));

        Assert.Contains("Cannot start streaming session", exception.Message);
    }

    [Theory]
    [InlineData("test-video.mp4", "test-video.mp4")]
    [InlineData("videos/test-video.mp4", "test-video.mp4")]
    [InlineData("https://example.com/videos/test-video.mp4", "test-video.mp4")]
    public async Task Handle_ShouldExtractVideoKeyCorrectly(string videoUrl, string expectedKey)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var query = new GetSecureVideoUrlQuery(userId, moduleId, 60);

        var accessResult = VideoAccessResult.Granted(DateTime.UtcNow.AddDays(15), 15);
        var module = CourseModule.Create(courseId, "Test Module", "وحدة اختبار", videoUrl, 60, 1);
        var streamingUrl = "https://secure-video-url.com";

        _mockVideoAccessService.Setup(s => s.ValidateModuleVideoAccessAsync(userId, moduleId))
            .ReturnsAsync(accessResult);
        _mockCourseRepository.Setup(r => r.GetModuleByIdAsync(moduleId, default))
            .ReturnsAsync(module);
        _mockVideoAccessService.Setup(s => s.CanStartStreamingSessionAsync(userId, moduleId))
            .ReturnsAsync(true);
        _mockVideoStreamingService.Setup(s => s.GenerateSecureStreamingUrlAsync(expectedKey, userId, 60))
            .ReturnsAsync(streamingUrl);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(streamingUrl, result.StreamingUrl);
        _mockVideoStreamingService.Verify(s => s.GenerateSecureStreamingUrlAsync(expectedKey, userId, 60), Times.Once);
    }
}