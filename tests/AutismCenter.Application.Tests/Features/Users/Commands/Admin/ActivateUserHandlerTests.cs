using Xunit;
using Moq;
using AutismCenter.Application.Features.Users.Commands.Admin.ActivateUser;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Domain.Enums;
using AutismCenter.Application.Common.Exceptions;

namespace AutismCenter.Application.Tests.Features.Users.Commands.Admin;

public class ActivateUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly ActivateUserHandler _handler;

    public ActivateUserHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new ActivateUserHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsActivateUserResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = Email.Create("test@example.com");
        var user = User.Create(email, "John", "Doe", UserRole.Patient);
        user.Deactivate(); // Start with deactivated user

        var command = new ActivateUserCommand(userId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("John Doe", result.FullName);
        Assert.True(result.IsActive);

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ActivateUserCommand(userId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AlreadyActiveUser_StillReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = Email.Create("test@example.com");
        var user = User.Create(email, "John", "Doe", UserRole.Patient);
        // User is active by default

        var command = new ActivateUserCommand(userId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsActive);
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}