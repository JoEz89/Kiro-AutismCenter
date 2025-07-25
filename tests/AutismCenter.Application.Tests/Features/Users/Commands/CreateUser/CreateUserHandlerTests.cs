using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Features.Users.Commands.CreateUser;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AutismCenter.Application.Tests.Features.Users.Commands.CreateUser;

public class CreateUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateUserHandler _handler;

    public CreateUserHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordServiceMock = new Mock<IPasswordService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateUserHandler(_userRepositoryMock.Object, _passwordServiceMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCreateUserSuccessfully()
    {
        // Arrange
        var command = new CreateUserCommand(
            "test@example.com",
            "John",
            "Doe",
            "Password123",
            "Patient",
            "en",
            "+1234567890"
        );

        var email = Email.Create(command.Email);
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordServiceMock.Setup(x => x.HashPassword(command.Password))
            .Returns("hashedPassword");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(command.Email);
        result.FirstName.Should().Be(command.FirstName);
        result.LastName.Should().Be(command.LastName);
        result.Role.Should().Be("Patient");
        result.PreferredLanguage.Should().Be("en");
        result.PhoneNumber.Should().Be(command.PhoneNumber);
        result.Message.Should().Be("User created successfully");

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new CreateUserCommand(
            "existing@example.com",
            "John",
            "Doe",
            "Password123"
        );

        var email = Email.Create(command.Email);
        var existingUser = User.Create(email, "Existing", "User");
        
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("A user with this email address already exists");
        
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithoutPassword_ShouldCreateUserWithoutPassword()
    {
        // Arrange
        var command = new CreateUserCommand(
            "test@example.com",
            "John",
            "Doe",
            null, // No password
            "Doctor",
            "ar"
        );

        var email = Email.Create(command.Email);
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be("Doctor");
        result.PreferredLanguage.Should().Be("ar");

        _passwordServiceMock.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Never);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("Patient", UserRole.Patient)]
    [InlineData("Doctor", UserRole.Doctor)]
    [InlineData("Admin", UserRole.Admin)]
    public async Task Handle_DifferentRoles_ShouldCreateUserWithCorrectRole(string roleString, UserRole expectedRole)
    {
        // Arrange
        var command = new CreateUserCommand(
            "test@example.com",
            "John",
            "Doe",
            "Password123",
            roleString
        );

        var email = Email.Create(command.Email);
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordServiceMock.Setup(x => x.HashPassword(command.Password))
            .Returns("hashedPassword");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Role.Should().Be(expectedRole.ToString());
    }

    [Theory]
    [InlineData("en", "en")]
    [InlineData("ar", "ar")]
    [InlineData("EN", "en")]
    [InlineData("AR", "ar")]
    public async Task Handle_DifferentLanguages_ShouldCreateUserWithCorrectLanguage(string languageInput, string expectedLanguage)
    {
        // Arrange
        var command = new CreateUserCommand(
            "test@example.com",
            "John",
            "Doe",
            "Password123",
            "Patient",
            languageInput
        );

        var email = Email.Create(command.Email);
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordServiceMock.Setup(x => x.HashPassword(command.Password))
            .Returns("hashedPassword");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.PreferredLanguage.Should().Be(expectedLanguage);
    }
}
