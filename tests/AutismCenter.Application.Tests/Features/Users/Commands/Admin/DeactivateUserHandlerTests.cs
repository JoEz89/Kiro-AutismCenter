using Xunit;
using Moq;
using AutismCenter.Application.Features.Users.Commands.Admin.DeactivateUser;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Domain.Enums;
using AutismCenter.Application.Common.Exceptions;

namespace AutismCenter.Application.Tests.Features.Users.Commands.Admin;

public class DeactivateUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly DeactivateUserHandler _handler;

    public DeactivateUserHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new DeactivateUserHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsDeactivateUserResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = Email.Create("test@example.com");
        var user = User.Create(email, "John", "Doe", UserRole.Patient);
        var command = new DeactivateUserCommand(userId, "Violation of terms");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("John Doe", result.FullName);
        Assert.False(result.IsActive);

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeactivateUserCommand(userId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AlreadyInactiveUser_StillReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = Email.Create("test@example.com");
        var user = User.Create(email, "John", "Doe", UserRole.Patient);
        user.Deactivate(); // Already deactivated

        var command = new DeactivateUserCommand(userId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsActive);
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}