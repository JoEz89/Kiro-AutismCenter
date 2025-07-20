using AutismCenter.Application.Features.Authentication.Commands.GoogleLogin;
using AutismCenter.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AutismCenter.Infrastructure.Tests.Features;

public class GoogleLoginTests
{
    [Fact]
    public void GoogleLoginCommand_ValidToken_CreatesCommand()
    {
        // Arrange
        var token = "valid-google-token";

        // Act
        var command = new GoogleLoginCommand(token);

        // Assert
        Assert.Equal(token, command.GoogleToken);
    }

    [Fact]
    public async Task GoogleAuthService_InvalidToken_ReturnsNull()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        var loggerMock = new Mock<ILogger<GoogleAuthService>>();
        
        configMock.Setup(x => x["GoogleAuth:ClientId"]).Returns("test-client-id");
        
        var service = new GoogleAuthService(configMock.Object, loggerMock.Object);

        // Act
        var result = await service.ValidateGoogleTokenAsync("invalid-token");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GoogleAuthService_EmptyToken_ReturnsNull()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        var loggerMock = new Mock<ILogger<GoogleAuthService>>();
        
        var service = new GoogleAuthService(configMock.Object, loggerMock.Object);

        // Act
        var result = await service.ValidateGoogleTokenAsync("");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GoogleAuthService_MissingClientId_ReturnsNull()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        var loggerMock = new Mock<ILogger<GoogleAuthService>>();
        
        configMock.Setup(x => x["GoogleAuth:ClientId"]).Returns((string?)null);
        
        var service = new GoogleAuthService(configMock.Object, loggerMock.Object);

        // Act
        var result = await service.ValidateGoogleTokenAsync("some-token");

        // Assert
        Assert.Null(result);
    }
}