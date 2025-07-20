using AutismCenter.Application.Features.Users.Queries.GetUser;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AutismCenter.Application.Tests.Features.Users.Queries.GetUser;

public class GetUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly GetUserHandler _handler;

    public GetUserHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new GetUserHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingUser_ShouldReturnUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserQuery(userId);

        var email = Email.Create("test@example.com");
        var phoneNumber = PhoneNumber.Create("+1234567890");
        var user = User.Create(email, "John", "Doe");
        user.UpdateProfile("John", "Doe", phoneNumber);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be("test@example.com");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.FullName.Should().Be("John Doe");
        result.Role.Should().Be("Patient");
        result.PreferredLanguage.Should().Be("en");
        result.IsEmailVerified.Should().BeFalse();
        result.HasGoogleAccount.Should().BeFalse();
        result.PhoneNumber.Should().Be("+1234567890");
    }

    [Fact]
    public async Task Handle_NonExistingUser_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserQuery(userId);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_GoogleUser_ShouldReturnCorrectGoogleAccountStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserQuery(userId);

        var email = Email.Create("test@example.com");
        var user = User.CreateWithGoogle(email, "John", "Doe", "google123");

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.HasGoogleAccount.Should().BeTrue();
        result.IsEmailVerified.Should().BeTrue(); // Google accounts are pre-verified
    }
}