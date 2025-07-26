using AutismCenter.Domain.Interfaces;
using AutismCenter.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;

namespace AutismCenter.Infrastructure.Tests.Services;

public class ZoomServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<ZoomService>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly ZoomService _zoomService;

    public ZoomServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<ZoomService>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);

        // Setup configuration
        _configurationMock.Setup(x => x["Zoom:ApiKey"]).Returns("test-api-key");
        _configurationMock.Setup(x => x["Zoom:ApiSecret"]).Returns("test-api-secret");

        _zoomService = new ZoomService(_httpClient, _configurationMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateMeetingAsync_ValidRequest_ShouldReturnZoomMeeting()
    {
        // Arrange
        var request = new ZoomMeetingRequest(
            "Test Meeting",
            DateTime.UtcNow.AddHours(1),
            60,
            "password123",
            true,
            false,
            "Test agenda");

        var responseData = new
        {
            id = 123456789L,
            topic = "Test Meeting",
            join_url = "https://zoom.us/j/123456789",
            start_url = "https://zoom.us/s/123456789",
            start_time = request.StartTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            duration = 60,
            password = "password123",
            status = "waiting"
        };

        var responseJson = JsonSerializer.Serialize(responseData);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(responseJson)
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _zoomService.CreateMeetingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("123456789");
        result.Topic.Should().Be("Test Meeting");
        result.JoinUrl.Should().Be("https://zoom.us/j/123456789");
        result.StartUrl.Should().Be("https://zoom.us/s/123456789");
        result.DurationInMinutes.Should().Be(60);
        result.Password.Should().Be("password123");
        result.Status.Should().Be(ZoomMeetingStatus.Waiting);
    }

    [Fact]
    public async Task CreateMeetingAsync_ApiError_ShouldThrowException()
    {
        // Arrange
        var request = new ZoomMeetingRequest(
            "Test Meeting",
            DateTime.UtcNow.AddHours(1),
            60);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Bad Request")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _zoomService.CreateMeetingAsync(request));

        exception.Message.Should().Contain("Failed to create Zoom meeting");
    }

    [Fact]
    public async Task GetMeetingAsync_ValidMeetingId_ShouldReturnZoomMeeting()
    {
        // Arrange
        var meetingId = "123456789";
        var responseData = new
        {
            id = 123456789L,
            topic = "Test Meeting",
            join_url = "https://zoom.us/j/123456789",
            start_url = "https://zoom.us/s/123456789",
            start_time = DateTime.UtcNow.AddHours(1).ToString("yyyy-MM-ddTHH:mm:ssZ"),
            duration = 60,
            password = "password123",
            status = "waiting"
        };

        var responseJson = JsonSerializer.Serialize(responseData);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson)
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _zoomService.GetMeetingAsync(meetingId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("123456789");
        result.Topic.Should().Be("Test Meeting");
        result.Status.Should().Be(ZoomMeetingStatus.Waiting);
    }

    [Fact]
    public async Task GetMeetingAsync_MeetingNotFound_ShouldThrowException()
    {
        // Arrange
        var meetingId = "nonexistent";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _zoomService.GetMeetingAsync(meetingId));

        exception.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task DeleteMeetingAsync_ValidMeetingId_ShouldCompleteSuccessfully()
    {
        // Arrange
        var meetingId = "123456789";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act & Assert
        await _zoomService.DeleteMeetingAsync(meetingId);
        // Should complete without throwing
    }

    [Fact]
    public void Constructor_MissingApiKey_ShouldThrowException()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["Zoom:ApiKey"]).Returns((string?)null);
        configMock.Setup(x => x["Zoom:ApiSecret"]).Returns("test-secret");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => new ZoomService(_httpClient, configMock.Object, _loggerMock.Object));

        exception.Message.Should().Contain("Zoom API Key not configured");
    }

    [Fact]
    public void Constructor_MissingApiSecret_ShouldThrowException()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["Zoom:ApiKey"]).Returns("test-key");
        configMock.Setup(x => x["Zoom:ApiSecret"]).Returns((string?)null);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => new ZoomService(_httpClient, configMock.Object, _loggerMock.Object));

        exception.Message.Should().Contain("Zoom API Secret not configured");
    }
}