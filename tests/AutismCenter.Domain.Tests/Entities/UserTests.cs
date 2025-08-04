using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Domain.Events;
using FluentAssertions;

namespace AutismCenter.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var user = User.Create(email, firstName, lastName);

        // Assert
        user.Should().NotBeNull();
        user.Email.Should().Be(email);
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.Role.Should().Be(UserRole.Patient);
        user.PreferredLanguage.Should().Be(Language.English);
        user.IsEmailVerified.Should().BeFalse();
        user.IsActive.Should().BeTrue();
        user.GetDomainEvents().Should().ContainSingle(e => e is UserCreatedEvent);
    }

    [Fact]
    public void Create_WithEmptyFirstName_ShouldThrowArgumentException()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var firstName = "";
        var lastName = "Doe";

        // Act & Assert
        var act = () => User.Create(email, firstName, lastName);
        act.Should().Throw<ArgumentException>()
           .WithMessage("First name cannot be empty*");
    }

    [Fact]
    public void Create_WithEmptyLastName_ShouldThrowArgumentException()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var firstName = "John";
        var lastName = "";

        // Act & Assert
        var act = () => User.Create(email, firstName, lastName);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Last name cannot be empty*");
    }

    [Fact]
    public void CreateWithGoogle_ShouldCreateUserWithGoogleId()
    {
        // Arrange
        var email = Email.Create("test@gmail.com");
        var firstName = "John";
        var lastName = "Doe";
        var googleId = "google123";

        // Act
        var user = User.CreateWithGoogle(email, firstName, lastName, googleId);

        // Assert
        user.Should().NotBeNull();
        user.GoogleId.Should().Be(googleId);
        user.IsEmailVerified.Should().BeTrue();
        user.HasGoogleAccount().Should().BeTrue();
    }

    [Fact]
    public void SetPassword_WithValidHash_ShouldSetPassword()
    {
        // Arrange
        var user = CreateTestUser();
        var passwordHash = "hashedPassword123";

        // Act
        user.SetPassword(passwordHash);

        // Assert
        user.PasswordHash.Should().Be(passwordHash);
    }

    [Fact]
    public void SetPassword_WithEmptyHash_ShouldThrowArgumentException()
    {
        // Arrange
        var user = CreateTestUser();

        // Act & Assert
        var act = () => user.SetPassword("");
        act.Should().Throw<ArgumentException>()
           .WithMessage("Password hash cannot be empty*");
    }

    [Fact]
    public void VerifyEmail_WhenNotVerified_ShouldVerifyEmail()
    {
        // Arrange
        var user = CreateTestUser();
        user.IsEmailVerified.Should().BeFalse();

        // Act
        user.VerifyEmail();

        // Assert
        user.IsEmailVerified.Should().BeTrue();
        user.GetDomainEvents().Should().Contain(e => e is UserEmailVerifiedEvent);
    }

    [Fact]
    public void VerifyEmail_WhenAlreadyVerified_ShouldNotAddEvent()
    {
        // Arrange
        var user = CreateTestUser();
        user.VerifyEmail();
        user.ClearDomainEvents();

        // Act
        user.VerifyEmail();

        // Assert
        user.IsEmailVerified.Should().BeTrue();
        user.GetDomainEvents().Should().BeEmpty();
    }

    [Fact]
    public void ChangeEmail_WithDifferentEmail_ShouldChangeEmailAndRequireVerification()
    {
        // Arrange
        var user = CreateTestUser();
        user.VerifyEmail();
        var newEmail = Email.Create("newemail@example.com");

        // Act
        user.ChangeEmail(newEmail);

        // Assert
        user.Email.Should().Be(newEmail);
        user.IsEmailVerified.Should().BeFalse();
        user.GetDomainEvents().Should().Contain(e => e is UserEmailChangedEvent);
    }

    [Fact]
    public void ChangeEmail_WithSameEmail_ShouldNotChangeAnything()
    {
        // Arrange
        var user = CreateTestUser();
        var originalEmail = user.Email;
        user.ClearDomainEvents();

        // Act
        user.ChangeEmail(originalEmail);

        // Assert
        user.Email.Should().Be(originalEmail);
        user.GetDomainEvents().Should().BeEmpty();
    }

    [Fact]
    public void UpdateProfile_WithValidData_ShouldUpdateProfile()
    {
        // Arrange
        var user = CreateTestUser();
        var newFirstName = "Jane";
        var newLastName = "Smith";
        var phoneNumber = PhoneNumber.Create("+1234567890");

        // Act
        user.UpdateProfile(newFirstName, newLastName, phoneNumber);

        // Assert
        user.FirstName.Should().Be(newFirstName);
        user.LastName.Should().Be(newLastName);
        user.PhoneNumber.Should().Be(phoneNumber);
        user.GetDomainEvents().Should().Contain(e => e is UserProfileUpdatedEvent);
    }

    [Fact]
    public void UpdateProfile_WithEmptyFirstName_ShouldThrowArgumentException()
    {
        // Arrange
        var user = CreateTestUser();

        // Act & Assert
        var act = () => user.UpdateProfile("", "Smith");
        act.Should().Throw<ArgumentException>()
           .WithMessage("First name cannot be empty*");
    }

    [Fact]
    public void ChangePreferredLanguage_WithDifferentLanguage_ShouldChangeLanguage()
    {
        // Arrange
        var user = CreateTestUser();
        var newLanguage = Language.Arabic;

        // Act
        user.ChangePreferredLanguage(newLanguage);

        // Assert
        user.PreferredLanguage.Should().Be(newLanguage);
    }

    [Fact]
    public void ChangeRole_WithDifferentRole_ShouldChangeRole()
    {
        // Arrange
        var user = CreateTestUser();
        var newRole = UserRole.Admin;

        // Act
        user.ChangeRole(newRole);

        // Assert
        user.Role.Should().Be(newRole);
        user.GetDomainEvents().Should().Contain(e => e is UserRoleChangedEvent);
    }

    [Fact]
    public void GetFullName_ShouldReturnConcatenatedName()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var fullName = user.GetFullName();

        // Assert
        fullName.Should().Be("John Doe");
    }

    [Fact]
    public void CanLogin_WithVerifiedEmailAndPassword_ShouldReturnTrue()
    {
        // Arrange
        var user = CreateTestUser();
        user.SetPassword("hashedPassword");
        user.VerifyEmail();

        // Act
        var canLogin = user.CanLogin();

        // Assert
        canLogin.Should().BeTrue();
    }

    [Fact]
    public void CanLogin_WithUnverifiedEmail_ShouldReturnFalse()
    {
        // Arrange
        var user = CreateTestUser();
        user.SetPassword("hashedPassword");

        // Act
        var canLogin = user.CanLogin();

        // Assert
        canLogin.Should().BeFalse();
    }

    [Fact]
    public void CanLogin_WithGoogleAccount_ShouldReturnTrue()
    {
        // Arrange
        var email = Email.Create("test@gmail.com");
        var user = User.CreateWithGoogle(email, "John", "Doe", "google123");

        // Act
        var canLogin = user.CanLogin();

        // Assert
        canLogin.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateUser()
    {
        // Arrange
        var user = CreateTestUser();
        user.IsActive.Should().BeTrue();

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
        user.GetDomainEvents().Should().Contain(e => e is UserDeactivatedEvent);
    }

    [Fact]
    public void Activate_WhenInactive_ShouldActivateUser()
    {
        // Arrange
        var user = CreateTestUser();
        user.Deactivate();
        user.ClearDomainEvents();

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
        user.GetDomainEvents().Should().Contain(e => e is UserActivatedEvent);
    }

    private static User CreateTestUser()
    {
        var email = Email.Create("test@example.com");
        return User.Create(email, "John", "Doe");
    }
}