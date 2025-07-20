using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Features.Users.Commands.ChangePassword;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AutismCenter.Application.Tests.Features.Users.Commands.ChangePassword;

public class ChangePasswordHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ChangePasswordHandler _handler;

    public ChangePasswordHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordServiceMock = new Mock<IPasswordService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new ChangePasswordHandler(_userRepositoryMock.Object, _passwordServiceMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldChangePasswordSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand(
            userId,
            "CurrentPassword123",
            "NewPassword123",
            "NewPassword123"
        );

        var email = Email.Create("test@example.com");
        var user = User.Create(email, "John", "Doe");
        user.SetPassword("hashedCurrentPassword");

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordServiceMock.Setup(x => x.VerifyPassword(command.CurrentPassword, "hashedCurrentPassword"))
            .Returns(true);

        _passwordServiceMock.Setup(x => x.IsValidPassword(command.NewPassword))
            .Returns(true);

        _passwordServiceMock.Setup(x => x.HashPassword(command.NewPassword))
            .Returns("hashedNewPassword");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Password changed successfully");

        _userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand(
            userId,
            "CurrentPassword123",
            "NewPassword123",
            "NewPassword123"
        );

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found");

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_GoogleOnlyAccount_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand(
            userId,
            "CurrentPassword123",
            "NewPassword123",
            "NewPassword123"
        );

        var email = Email.Create("test@example.com");
        var user = User.CreateWithGoogle(email, "John", "Doe", "google123");

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Cannot change password for Google-authenticated accounts");

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_IncorrectCurrentPassword_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand(
            userId,
            "WrongCurrentPassword",
            "NewPassword123",
            "NewPassword123"
        );

        var email = Email.Create("test@example.com");
        var user = User.Create(email, "John", "Doe");
        user.SetPassword("hashedCurrentPassword");

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordServiceMock.Setup(x => x.VerifyPassword(command.CurrentPassword, "hashedCurrentPassword"))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Current password is incorrect");

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InvalidNewPassword_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand(
            userId,
            "CurrentPassword123",
            "weak",
            "weak"
        );

        var email = Email.Create("test@example.com");
        var user = User.Create(email, "John", "Doe");
        user.SetPassword("hashedCurrentPassword");

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordServiceMock.Setup(x => x.VerifyPassword(command.CurrentPassword, "hashedCurrentPassword"))
            .Returns(true);

        _passwordServiceMock.Setup(x => x.IsValidPassword(command.NewPassword))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("New password does not meet complexity requirements");

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}