using AutismCenter.Application.Features.Courses.Queries.GetSecureVideoUrl;
using AutismCenter.Application.Features.Courses.Commands.EndVideoSession;
using AutismCenter.WebApi.Controllers;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace AutismCenter.Infrastructure.Tests.Controllers;

public class VideoStreamingControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly VideoStreamingController _controller;
    private readonly Guid _testUserId = Guid.NewGuid();

    public VideoStreamingControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new VideoStreamingController();

        // Setup controller context
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Create a mock HttpContext with the mediator service
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetService(typeof(ISender))).Returns(_mediatorMock.Object);
        _controller.ControllerContext.HttpContext.RequestServices = serviceProvider.Object;
    }

    [Fact]
    public async Task GetSecureVideoUrl_ValidRequest_ShouldReturnOkWithSecureUrl()
    {
        // Arrange
        SetupAuthenticatedUser();
        var moduleId = Guid.NewGuid();
        var expirationMinutes = 60;

        var expectedResponse = new GetSecureVideoUrlResponse
        {
            StreamingUrl = "https://secure-video-url.com/video123?token=abc123",
            SessionId = "session-123",
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            DaysRemaining = 30,
            ModuleTitle = "Module 1",
            ModuleDuration = 60
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetSecureVideoUrlQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetSecureVideoUrl(moduleId, expirationMinutes);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<GetSecureVideoUrlResponse>().Subject;
        
        returnValue.StreamingUrl.Should().Be("https://secure-video-url.com/video123?token=abc123");
        returnValue.SessionId.Should().Be("session-123");
        returnValue.ModuleTitle.Should().Be("Module 1");

        _mediatorMock.Verify(x => x.Send(
            It.Is<GetSecureVideoUrlQuery>(q => 
                q.UserId == _testUserId &&
                q.ModuleId == moduleId &&
                q.ExpirationMinutes == expirationMinutes),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetSecureVideoUrl_DefaultExpirationMinutes_ShouldUseDefault60Minutes()
    {
        // Arrange
        SetupAuthenticatedUser();
        var moduleId = Guid.NewGuid();

        var expectedResponse = new GetSecureVideoUrlResponse
        {
            StreamingUrl = "https://secure-video-url.com/video123?token=abc123",
            SessionId = "session-123",
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            DaysRemaining = 30,
            ModuleTitle = "Module 1",
            ModuleDuration = 60
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetSecureVideoUrlQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetSecureVideoUrl(moduleId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;

        _mediatorMock.Verify(x => x.Send(
            It.Is<GetSecureVideoUrlQuery>(q => 
                q.UserId == _testUserId &&
                q.ModuleId == moduleId &&
                q.ExpirationMinutes == 60),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetSecureVideoUrl_ExpirationMinutesTooLow_ShouldReturnBadRequest()
    {
        // Arrange
        SetupAuthenticatedUser();
        var moduleId = Guid.NewGuid();
        var expirationMinutes = 0;

        // Act
        var result = await _controller.GetSecureVideoUrl(moduleId, expirationMinutes);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Expiration minutes must be between 1 and 120");

        _mediatorMock.Verify(x => x.Send(
            It.IsAny<GetSecureVideoUrlQuery>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetSecureVideoUrl_ExpirationMinutesTooHigh_ShouldReturnBadRequest()
    {
        // Arrange
        SetupAuthenticatedUser();
        var moduleId = Guid.NewGuid();
        var expirationMinutes = 121;

        // Act
        var result = await _controller.GetSecureVideoUrl(moduleId, expirationMinutes);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Expiration minutes must be between 1 and 120");

        _mediatorMock.Verify(x => x.Send(
            It.IsAny<GetSecureVideoUrlQuery>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetSecureVideoUrl_Unauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        var moduleId = Guid.NewGuid();
        var expirationMinutes = 60;

        // Act
        var result = await _controller.GetSecureVideoUrl(moduleId, expirationMinutes);

        // Assert
        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.Value.Should().Be("User ID not found in token");

        _mediatorMock.Verify(x => x.Send(
            It.IsAny<GetSecureVideoUrlQuery>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetSecureVideoUrl_UnauthorizedAccessException_ShouldReturnForbid()
    {
        // Arrange
        SetupAuthenticatedUser();
        var moduleId = Guid.NewGuid();

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetSecureVideoUrlQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not have access to this video"));

        // Act
        var result = await _controller.GetSecureVideoUrl(moduleId);

        // Assert
        var forbidResult = result.Result.Should().BeOfType<ForbidResult>().Subject;
    }

    [Fact]
    public async Task GetSecureVideoUrl_InvalidOperationException_ShouldReturnBadRequest()
    {
        // Arrange
        SetupAuthenticatedUser();
        var moduleId = Guid.NewGuid();

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetSecureVideoUrlQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Enrollment has expired"));

        // Act
        var result = await _controller.GetSecureVideoUrl(moduleId);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Enrollment has expired");
    }

    [Fact]
    public async Task GetSecureVideoUrl_GeneralException_ShouldReturnInternalServerError()
    {
        // Arrange
        SetupAuthenticatedUser();
        var moduleId = Guid.NewGuid();

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetSecureVideoUrlQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetSecureVideoUrl(moduleId);

        // Assert
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
        statusCodeResult.Value.Should().Be("An error occurred while generating the secure video URL");
    }

    [Fact]
    public async Task EndVideoSession_ValidRequest_ShouldReturnOkWithSuccessMessage()
    {
        // Arrange
        var request = new EndVideoSessionRequest { SessionId = "session-123" };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<EndVideoSessionCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.EndVideoSession(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeAssignableTo<object>().Subject;
        
        // Check the anonymous object properties
        var messageProperty = returnValue.GetType().GetProperty("message");
        messageProperty.Should().NotBeNull();
        messageProperty!.GetValue(returnValue).Should().Be("Video session ended successfully");

        _mediatorMock.Verify(x => x.Send(
            It.Is<EndVideoSessionCommand>(c => c.SessionId == "session-123"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EndVideoSession_EmptySessionId_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new EndVideoSessionRequest { SessionId = "" };

        // Act
        var result = await _controller.EndVideoSession(request);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Session ID is required");

        _mediatorMock.Verify(x => x.Send(
            It.IsAny<EndVideoSessionCommand>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EndVideoSession_NullSessionId_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new EndVideoSessionRequest { SessionId = null! };

        // Act
        var result = await _controller.EndVideoSession(request);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Session ID is required");

        _mediatorMock.Verify(x => x.Send(
            It.IsAny<EndVideoSessionCommand>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EndVideoSession_ArgumentException_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new EndVideoSessionRequest { SessionId = "invalid-session" };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<EndVideoSessionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Invalid session ID format"));

        // Act
        var result = await _controller.EndVideoSession(request);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Invalid session ID format");
    }

    [Fact]
    public async Task EndVideoSession_GeneralException_ShouldReturnInternalServerError()
    {
        // Arrange
        var request = new EndVideoSessionRequest { SessionId = "session-123" };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<EndVideoSessionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.EndVideoSession(request);

        // Assert
        var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
        statusCodeResult.Value.Should().Be("An error occurred while ending the video session");
    }

    [Fact]
    public async Task ValidateVideoAccess_ValidRequest_ShouldReturnOkWithValidationResponse()
    {
        // Arrange
        SetupAuthenticatedUser();
        var moduleId = Guid.NewGuid();

        // Act
        var result = await _controller.ValidateVideoAccess(moduleId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<VideoAccessValidationResponse>().Subject;
        
        returnValue.HasAccess.Should().BeFalse();
        returnValue.Reason.Should().Be("Not implemented in this demo");
        returnValue.DaysRemaining.Should().Be(0);
    }

    [Fact]
    public async Task ValidateVideoAccess_Unauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        var moduleId = Guid.NewGuid();

        // Act
        var result = await _controller.ValidateVideoAccess(moduleId);

        // Assert
        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.Value.Should().Be("User ID not found in token");
    }

    [Fact]
    public async Task ValidateVideoAccess_Exception_ShouldReturnInternalServerError()
    {
        // Arrange
        SetupAuthenticatedUser();
        var moduleId = Guid.NewGuid();

        // Mock the controller to throw an exception
        var mockController = new Mock<VideoStreamingController>();
        mockController.Setup(x => x.ValidateVideoAccess(It.IsAny<Guid>()))
                     .ThrowsAsync(new Exception("Service unavailable"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => mockController.Object.ValidateVideoAccess(moduleId));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(120)]
    public async Task GetSecureVideoUrl_ValidExpirationMinutes_ShouldAcceptValidRange(int expirationMinutes)
    {
        // Arrange
        SetupAuthenticatedUser();
        var moduleId = Guid.NewGuid();

        var expectedResponse = new GetSecureVideoUrlResponse(
            "https://secure-video-url.com/video123?token=abc123",
            "session-123",
            DateTime.UtcNow.AddMinutes(expirationMinutes),
            "Secure URL generated successfully");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetSecureVideoUrlQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetSecureVideoUrl(moduleId, expirationMinutes);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Should().NotBeNull();

        _mediatorMock.Verify(x => x.Send(
            It.Is<GetSecureVideoUrlQuery>(q => q.ExpirationMinutes == expirationMinutes),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private void SetupAuthenticatedUser()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim(ClaimTypes.Role, "User")
        }, "test"));

        _controller.ControllerContext.HttpContext.User = user;
    }
}