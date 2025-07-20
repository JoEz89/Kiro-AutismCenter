using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Features.Users.Commands.UpdateUser;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AutismCenter.Application.Tests.Features.Users.Commands.UpdateUser;

public class UpdateUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateUserHandler _handler;

    public UpdateUserHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateUserHandler(_userRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldUpdateUserSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand(
            userId,
            "UpdatedFirst",
            "UpdatedLast",
            "+9876543210",
            "ar"
        );

        var email = Email.Create("test@example.com");
        var user = User.Create(email, "Original", "Name");

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.FirstName.Should().Be("UpdatedFirst");
        result.LastName.Should().Be("UpdatedLast");
        result.PhoneNumber.Should().Be("+9876543210");
        result.PreferredLanguage.Should().Be("ar");
        result.Message.Should().Be("User updated successfully");

        _userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand(
            userId,
            "UpdatedFirst",
            "UpdatedLast"
        );

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("User not found");
        
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithoutPhoneNumber_ShouldUpdateUserWithoutPhoneNumber()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand(
            userId,
            "UpdatedFirst",
            "UpdatedLast"
        );

        var email = Email.Create("test@example.com");
        var user = User.Create(email, "Original", "Name");

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PhoneNumber.Should().BeNull();
        result.PreferredLanguage.Should().Be("en"); // Should remain unchanged
    }

    [Fact]
    public async Task Handle_WithoutPreferredLanguage_ShouldNotChangeLanguage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand(
            userId,
            "UpdatedFirst",
            "UpdatedLast",
            "+1234567890"
        );

        var email = Email.Create("test@example.com");
        var user = User.Create(email, "Original", "Name", UserRole.Patient, Language.Arabic);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PreferredLanguage.Should().Be("ar"); // Should remain Arabic
    }

    [Theory]
    [InlineData("en", "en")]
    [InlineData("ar", "ar")]
    [InlineData("EN", "en")]
    [InlineData("AR", "ar")]
    public async Task Handle_DifferentLanguages_ShouldUpdateLanguageCorrectly(string languageInput, string expectedLanguage)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand(
            userId,
            "UpdatedFirst",
            "UpdatedLast",
            null,
            languageInput
        );

        var email = Email.Create("test@example.com");
        var user = User.Create(email, "Original", "Name");

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.PreferredLanguage.Should().Be(expectedLanguage);
    }
}