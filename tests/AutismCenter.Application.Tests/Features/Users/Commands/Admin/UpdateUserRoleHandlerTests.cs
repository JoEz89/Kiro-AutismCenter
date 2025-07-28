using Xunit;
using Moq;
using AutismCenter.Application.Features.Users.Commands.Admin.UpdateUserRole;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Domain.Enums;
using AutismCenter.Application.Common.Exceptions;

namespace AutismCenter.Application.Tests.Features.Users.Commands.Admin;

public class UpdateUserRoleHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly UpdateUserRoleHandler _handler;

    public UpdateUserRoleHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new UpdateUserRoleHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsUpdateUserRoleResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = Email.Create("test@example.com");
        var user = User.Create(email, "John", "Doe", UserRole.Patient);
        var command = new UpdateUserRoleCommand(userId, UserRole.Admin);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("John Doe", result.FullName);
        Assert.Equal(UserRole.Admin, result.Role);

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserRoleCommand(userId, UserRole.Admin);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ChangeFromPatientToDoctor_UpdatesRoleCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = Email.Create("doctor@example.com");
        var user = User.Create(email, "Jane", "Smith", UserRole.Patient);
        var command = new UpdateUserRoleCommand(userId, UserRole.Doctor);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(UserRole.Doctor, result.Role);
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}