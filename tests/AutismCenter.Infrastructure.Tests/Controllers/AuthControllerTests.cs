using AutismCenter.Application.Features.Authentication.Commands.Login;
using AutismCenter.Application.Features.Authentication.Commands.RegisterUser;
using AutismCenter.Application.Features.Authentication.Commands.RefreshToken;
using AutismCenter.Application.Features.Authentication.Commands.Logout;
using AutismCenter.Application.Features.Authentication.Commands.VerifyEmail;
using AutismCenter.Application.Features.Authentication.Commands.GoogleLogin;
using AutismCenter.Application.Features.Authentication.Commands.ForgotPassword;
using AutismCenter.Application.Features.Authentication.Commands.ResetPassword;
using AutismCenter.Application.Common.Models;
using AutismCenter.WebApi.Controllers;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace AutismCenter.Infrastructure.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new AuthController();

        // Setup controller context
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Use reflection to set the mediator since it's accessed through the base class
        var mediatorProperty = typeof(BaseController).GetProperty("Mediator", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        // Create a mock HttpContext with the mediator service
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetService(typeof(ISender))).Returns(_mediatorMock.Object);
        _controller.ControllerContext.HttpContext.RequestServices = serviceProvider.Object;
    }

    [Fact]
    public async Task Login_ValidCredentials_ShouldReturnOkWithAuthResult()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "password123");
        var expectedResult = new AuthenticationResult(
            Guid.NewGuid(),
            "test@example.com",
            "John",
            "Doe",
            "User",
            "access_token",
            "refresh_token",
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddDays(7)
        );

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Login(command);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<AuthenticationResult>().Subject;
        
        returnValue.Email.Should().Be("test@example.com");
        returnValue.FirstName.Should().Be("John");
        returnValue.LastName.Should().Be("Doe");
        returnValue.AccessToken.Should().Be("access_token");

        _mediatorMock.Verify(x => x.Send(
            It.Is<LoginCommand>(c => c.Email == "test@example.com" && c.Password == "password123"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "wrongpassword");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

        // Act
        var result = await _controller.Login(command);

        // Assert
        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var errorMessage = unauthorizedResult.Value.Should().BeEquivalentTo(new { message = "Invalid credentials" });
    }

    [Fact]
    public async Task Login_InvalidInput_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new LoginCommand("", "password123");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Email is required"));

        // Act
        var result = await _controller.Login(command);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var errorMessage = badRequestResult.Value.Should().BeEquivalentTo(new { message = "Email is required" });
    }

    [Fact]
    public async Task Register_ValidRequest_ShouldReturnOkWithResponse()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "newuser@example.com",
            "John",
            "Doe",
            "password123",
            "password123",
            "en"
        );
        var expectedResult = new RegisterUserResponse(
            Guid.NewGuid(),
            "newuser@example.com",
            "Registration successful. Please check your email for verification."
        );

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Register(command);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<RegisterUserResponse>().Subject;
        
        returnValue.Email.Should().Be("newuser@example.com");
        returnValue.Message.Should().Be("Registration successful. Please check your email for verification.");

        _mediatorMock.Verify(x => x.Send(
            It.Is<RegisterUserCommand>(c => 
                c.Email == "newuser@example.com" &&
                c.FirstName == "John" &&
                c.LastName == "Doe" &&
                c.Password == "password123" &&
                c.ConfirmPassword == "password123" &&
                c.PreferredLanguage == "en"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "existing@example.com",
            "John",
            "Doe",
            "password123",
            "password123",
            "en"
        );

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("User with this email already exists"));

        // Act
        var result = await _controller.Register(command);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var errorMessage = badRequestResult.Value.Should().BeEquivalentTo(new { message = "User with this email already exists" });
    }

    [Fact]
    public async Task RefreshToken_ValidToken_ShouldReturnOkWithNewTokens()
    {
        // Arrange
        var command = new RefreshTokenCommand("valid_refresh_token");
        var expectedResult = new AuthenticationResult(
            Guid.NewGuid(),
            "test@example.com",
            "John",
            "Doe",
            "User",
            "new_access_token",
            "new_refresh_token",
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddDays(7)
        );

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.RefreshToken(command);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<AuthenticationResult>().Subject;
        
        returnValue.AccessToken.Should().Be("new_access_token");
        returnValue.RefreshToken.Should().Be("new_refresh_token");

        _mediatorMock.Verify(x => x.Send(
            It.Is<RefreshTokenCommand>(c => c.RefreshToken == "valid_refresh_token"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshToken_InvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var command = new RefreshTokenCommand("invalid_refresh_token");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid refresh token"));

        // Act
        var result = await _controller.RefreshToken(command);

        // Assert
        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var errorMessage = unauthorizedResult.Value.Should().BeEquivalentTo(new { message = "Invalid refresh token" });
    }

    [Fact]
    public async Task Logout_ValidRequest_ShouldReturnOkWithResponse()
    {
        // Arrange
        var command = new LogoutCommand("refresh_token_to_revoke");
        var expectedResult = new LogoutResponse(true, "Logged out successfully");

        // Setup authenticated user
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        }, "test"));

        _controller.ControllerContext.HttpContext.User = user;

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<LogoutCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Logout(command);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<LogoutResponse>().Subject;
        
        returnValue.Success.Should().BeTrue();
        returnValue.Message.Should().Be("Logged out successfully");

        _mediatorMock.Verify(x => x.Send(
            It.Is<LogoutCommand>(c => c.RefreshToken == "refresh_token_to_revoke"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyEmail_ValidToken_ShouldReturnOkWithSuccessResponse()
    {
        // Arrange
        var command = new VerifyEmailCommand("valid_verification_token");
        var expectedResult = new VerifyEmailResponse(true, "Email verified successfully");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<VerifyEmailCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.VerifyEmail(command);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<VerifyEmailResponse>().Subject;
        
        returnValue.Success.Should().BeTrue();
        returnValue.Message.Should().Be("Email verified successfully");

        _mediatorMock.Verify(x => x.Send(
            It.Is<VerifyEmailCommand>(c => c.Token == "valid_verification_token"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyEmail_InvalidToken_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new VerifyEmailCommand("invalid_verification_token");
        var expectedResult = new VerifyEmailResponse(false, "Invalid or expired verification token");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<VerifyEmailCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.VerifyEmail(command);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var returnValue = badRequestResult.Value.Should().BeOfType<VerifyEmailResponse>().Subject;
        
        returnValue.Success.Should().BeFalse();
        returnValue.Message.Should().Be("Invalid or expired verification token");
    }

    [Fact]
    public async Task VerifyEmailGet_ValidToken_ShouldReturnOkWithSuccessResponse()
    {
        // Arrange
        var token = "valid_verification_token";
        var expectedResult = new VerifyEmailResponse(true, "Email verified successfully");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<VerifyEmailCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.VerifyEmailGet(token);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<VerifyEmailResponse>().Subject;
        
        returnValue.Success.Should().BeTrue();
        returnValue.Message.Should().Be("Email verified successfully");

        _mediatorMock.Verify(x => x.Send(
            It.Is<VerifyEmailCommand>(c => c.Token == token),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GoogleLogin_ValidToken_ShouldReturnOkWithAuthResult()
    {
        // Arrange
        var command = new GoogleLoginCommand("valid_google_token");
        var expectedResult = new AuthenticationResult(
            Guid.NewGuid(),
            "google@example.com",
            "Google",
            "User",
            "User",
            "access_token",
            "refresh_token",
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddDays(7)
        );

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GoogleLoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GoogleLogin(command);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<AuthenticationResult>().Subject;
        
        returnValue.Email.Should().Be("google@example.com");
        returnValue.FirstName.Should().Be("Google");
        returnValue.LastName.Should().Be("User");

        _mediatorMock.Verify(x => x.Send(
            It.Is<GoogleLoginCommand>(c => c.GoogleToken == "valid_google_token"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GoogleLogin_InvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var command = new GoogleLoginCommand("invalid_google_token");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GoogleLoginCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid Google token"));

        // Act
        var result = await _controller.GoogleLogin(command);

        // Assert
        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var errorMessage = unauthorizedResult.Value.Should().BeEquivalentTo(new { message = "Invalid Google token" });
    }

    [Fact]
    public async Task ForgotPassword_ValidEmail_ShouldReturnOkWithResponse()
    {
        // Arrange
        var command = new ForgotPasswordCommand("test@example.com");
        var expectedResult = new ForgotPasswordResponse(true, "Password reset email sent successfully");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<ForgotPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.ForgotPassword(command);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<ForgotPasswordResponse>().Subject;
        
        returnValue.Success.Should().BeTrue();
        returnValue.Message.Should().Be("Password reset email sent successfully");

        _mediatorMock.Verify(x => x.Send(
            It.Is<ForgotPasswordCommand>(c => c.Email == "test@example.com"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ForgotPassword_InvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new ForgotPasswordCommand("invalid-email");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<ForgotPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Invalid email format"));

        // Act
        var result = await _controller.ForgotPassword(command);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var errorMessage = badRequestResult.Value.Should().BeEquivalentTo(new { message = "Invalid email format" });
    }

    [Fact]
    public async Task ResetPassword_ValidRequest_ShouldReturnOkWithSuccessResponse()
    {
        // Arrange
        var command = new ResetPasswordCommand(
            "valid_reset_token",
            "newpassword123",
            "newpassword123"
        );
        var expectedResult = new ResetPasswordResponse(true, "Password reset successfully");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.ResetPassword(command);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<ResetPasswordResponse>().Subject;
        
        returnValue.Success.Should().BeTrue();
        returnValue.Message.Should().Be("Password reset successfully");

        _mediatorMock.Verify(x => x.Send(
            It.Is<ResetPasswordCommand>(c => 
                c.Token == "valid_reset_token" &&
                c.NewPassword == "newpassword123" &&
                c.ConfirmPassword == "newpassword123"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_InvalidToken_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new ResetPasswordCommand(
            "invalid_reset_token",
            "newpassword123",
            "newpassword123"
        );
        var expectedResult = new ResetPasswordResponse(false, "Invalid or expired reset token");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.ResetPassword(command);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var returnValue = badRequestResult.Value.Should().BeOfType<ResetPasswordResponse>().Subject;
        
        returnValue.Success.Should().BeFalse();
        returnValue.Message.Should().Be("Invalid or expired reset token");
    }

    [Fact]
    public async Task ResetPassword_PasswordMismatch_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new ResetPasswordCommand(
            "valid_reset_token",
            "newpassword123",
            "differentpassword123"
        );

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Passwords do not match"));

        // Act
        var result = await _controller.ResetPassword(command);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var errorMessage = badRequestResult.Value.Should().BeEquivalentTo(new { message = "Passwords do not match" });
    }
}