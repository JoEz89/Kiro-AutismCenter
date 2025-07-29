using AutismCenter.Application.Features.Users.Commands.UpdateUser;
using AutismCenter.Application.Features.Users.Commands.ChangePassword;
using AutismCenter.Application.Features.Users.Queries.GetUser;
using AutismCenter.Application.Features.Users.Queries.GetUserProfile;
using AutismCenter.Application.Features.Users.Common;
using AutismCenter.WebApi.Controllers;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace AutismCenter.Infrastructure.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly UsersController _controller;
    private readonly Guid _testUserId;

    public UsersControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new UsersController();
        _testUserId = Guid.NewGuid();

        // Setup controller context with authenticated user
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim(ClaimTypes.Role, "User")
        }, "test"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Create a mock HttpContext with the mediator service
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetService(typeof(ISender))).Returns(_mediatorMock.Object);
        _controller.ControllerContext.HttpContext.RequestServices = serviceProvider.Object;
    }

    [Fact]
    public async Task GetProfile_ValidUser_ShouldReturnOkWithUserProfile()
    {
        // Arrange
        var expectedProfile = new UserProfileDto(
            _testUserId,
            "test@example.com",
            "John",
            "Doe",
            "John Doe",
            "User",
            "en",
            true,
            false,
            "+1234567890",
            5,
            3,
            2,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow
        );

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetUserProfileQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProfile);

        // Act
        var result = await _controller.GetProfile();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<UserProfileDto>().Subject;
        
        returnValue.Id.Should().Be(_testUserId);
        returnValue.Email.Should().Be("test@example.com");
        returnValue.FirstName.Should().Be("John");
        returnValue.LastName.Should().Be("Doe");
        returnValue.TotalOrders.Should().Be(5);
        returnValue.TotalEnrollments.Should().Be(3);
        returnValue.TotalAppointments.Should().Be(2);

        _mediatorMock.Verify(x => x.Send(
            It.Is<GetUserProfileQuery>(q => q.UserId == _testUserId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProfile_UserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetUserProfileQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfileDto?)null);

        // Act
        var result = await _controller.GetProfile();

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var errorMessage = notFoundResult.Value.Should().BeEquivalentTo(new { message = "User profile not found" });
    }

    [Fact]
    public async Task GetProfile_UnauthorizedUser_ShouldReturnUnauthorized()
    {
        // Arrange
        // Setup controller without user claims
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = await _controller.GetProfile();

        // Assert
        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var errorMessage = unauthorizedResult.Value.Should().BeEquivalentTo(new { message = "Invalid or missing user ID in token" });
    }

    [Fact]
    public async Task GetUser_ValidUserId_ShouldReturnOkWithUser()
    {
        // Arrange
        var targetUserId = Guid.NewGuid();
        var expectedUser = new UserDto(
            targetUserId,
            "target@example.com",
            "Jane",
            "Smith",
            "Jane Smith",
            "User",
            "en",
            true,
            false,
            "+9876543210",
            DateTime.UtcNow.AddDays(-60),
            DateTime.UtcNow
        );

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _controller.GetUser(targetUserId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<UserDto>().Subject;
        
        returnValue.Id.Should().Be(targetUserId);
        returnValue.Email.Should().Be("target@example.com");
        returnValue.FirstName.Should().Be("Jane");
        returnValue.LastName.Should().Be("Smith");

        _mediatorMock.Verify(x => x.Send(
            It.Is<GetUserQuery>(q => q.UserId == targetUserId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUser_UserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var targetUserId = Guid.NewGuid();
        
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _controller.GetUser(targetUserId);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var errorMessage = notFoundResult.Value.Should().BeEquivalentTo(new { message = "User not found" });
    }

    [Fact]
    public async Task UpdateProfile_ValidRequest_ShouldReturnOkWithUpdatedProfile()
    {
        // Arrange
        var request = new UpdateUserProfileRequest(
            "John Updated",
            "Doe Updated",
            "+1111111111",
            "ar"
        );

        var expectedResponse = new UpdateUserResponse(
            _testUserId,
            "John Updated",
            "Doe Updated",
            "+1111111111",
            "ar",
            DateTime.UtcNow,
            "Profile updated successfully"
        );

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.UpdateProfile(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<UpdateUserResponse>().Subject;
        
        returnValue.Id.Should().Be(_testUserId);
        returnValue.FirstName.Should().Be("John Updated");
        returnValue.LastName.Should().Be("Doe Updated");
        returnValue.PhoneNumber.Should().Be("+1111111111");
        returnValue.PreferredLanguage.Should().Be("ar");

        _mediatorMock.Verify(x => x.Send(
            It.Is<UpdateUserCommand>(c => 
                c.UserId == _testUserId &&
                c.FirstName == "John Updated" &&
                c.LastName == "Doe Updated" &&
                c.PhoneNumber == "+1111111111" &&
                c.PreferredLanguage == "ar"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfile_InvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new UpdateUserProfileRequest(
            "", // Empty first name
            "Doe",
            "+1111111111",
            "en"
        );

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("First name is required"));

        // Act
        var result = await _controller.UpdateProfile(request);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var errorMessage = badRequestResult.Value.Should().BeEquivalentTo(new { message = "First name is required" });
    }

    [Fact]
    public async Task UpdateProfile_UnauthorizedUser_ShouldReturnUnauthorized()
    {
        // Arrange
        // Setup controller without user claims
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        var request = new UpdateUserProfileRequest(
            "John",
            "Doe",
            "+1111111111",
            "en"
        );

        // Act
        var result = await _controller.UpdateProfile(request);

        // Assert
        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var errorMessage = unauthorizedResult.Value.Should().BeEquivalentTo(new { message = "Invalid or missing user ID in token" });
    }

    [Fact]
    public async Task ChangePassword_ValidRequest_ShouldReturnOkWithSuccessResponse()
    {
        // Arrange
        var request = new ChangePasswordRequest(
            "currentpassword123",
            "newpassword123",
            "newpassword123"
        );

        var expectedResponse = new ChangePasswordResponse(
            true,
            "Password changed successfully"
        );

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<ChangePasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ChangePassword(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<ChangePasswordResponse>().Subject;
        
        returnValue.Success.Should().BeTrue();
        returnValue.Message.Should().Be("Password changed successfully");

        _mediatorMock.Verify(x => x.Send(
            It.Is<ChangePasswordCommand>(c => 
                c.UserId == _testUserId &&
                c.CurrentPassword == "currentpassword123" &&
                c.NewPassword == "newpassword123" &&
                c.ConfirmNewPassword == "newpassword123"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangePassword_WrongCurrentPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new ChangePasswordRequest(
            "wrongcurrentpassword",
            "newpassword123",
            "newpassword123"
        );

        var expectedResponse = new ChangePasswordResponse(
            false,
            "Current password is incorrect"
        );

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<ChangePasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ChangePassword(request);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var returnValue = badRequestResult.Value.Should().BeOfType<ChangePasswordResponse>().Subject;
        
        returnValue.Success.Should().BeFalse();
        returnValue.Message.Should().Be("Current password is incorrect");
    }

    [Fact]
    public async Task ChangePassword_PasswordMismatch_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new ChangePasswordRequest(
            "currentpassword123",
            "newpassword123",
            "differentpassword123"
        );

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<ChangePasswordCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("New passwords do not match"));

        // Act
        var result = await _controller.ChangePassword(request);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var errorMessage = badRequestResult.Value.Should().BeEquivalentTo(new { message = "New passwords do not match" });
    }

    [Fact]
    public async Task ChangePassword_UnauthorizedUser_ShouldReturnUnauthorized()
    {
        // Arrange
        // Setup controller without user claims
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        var request = new ChangePasswordRequest(
            "currentpassword123",
            "newpassword123",
            "newpassword123"
        );

        // Act
        var result = await _controller.ChangePassword(request);

        // Assert
        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var errorMessage = unauthorizedResult.Value.Should().BeEquivalentTo(new { message = "Invalid or missing user ID in token" });
    }

    [Fact]
    public async Task ChangePassword_WeakPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new ChangePasswordRequest(
            "currentpassword123",
            "weak", // Weak password
            "weak"
        );

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<ChangePasswordCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Password does not meet complexity requirements"));

        // Act
        var result = await _controller.ChangePassword(request);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var errorMessage = badRequestResult.Value.Should().BeEquivalentTo(new { message = "Password does not meet complexity requirements" });
    }
}