using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Domain.Events;

namespace AutismCenter.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnUser()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var user = User.Create(email, firstName, lastName);

        // Assert
        Assert.Equal(email, user.Email);
        Assert.Equal(firstName, user.FirstName);
        Assert.Equal(lastName, user.LastName);
        Assert.Equal(UserRole.Patient, user.Role);
        Assert.Equal(Language.English, user.PreferredLanguage);
        Assert.False(user.IsEmailVerified);
        Assert.Null(user.GoogleId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyFirstName_ShouldThrowArgumentException(string firstName)
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var lastName = "Doe";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => User.Create(email, firstName, lastName));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyLastName_ShouldThrowArgumentException(string lastName)
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var firstName = "John";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => User.Create(email, firstName, lastName));
    }

    [Fact]
    public void Create_ShouldAddUserCreatedEvent()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var user = User.Create(email, firstName, lastName);

        // Assert
        Assert.Single(user.DomainEvents);
        Assert.IsType<UserCreatedEvent>(user.DomainEvents.First());
    }

    [Fact]
    public void CreateWithGoogle_ShouldSetGoogleIdAndVerifyEmail()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var firstName = "John";
        var lastName = "Doe";
        var googleId = "google123";

        // Act
        var user = User.CreateWithGoogle(email, firstName, lastName, googleId);

        // Assert
        Assert.Equal(googleId, user.GoogleId);
        Assert.True(user.IsEmailVerified);
        Assert.True(user.HasGoogleAccount());
    }

    [Fact]
    public void VerifyEmail_WhenNotVerified_ShouldSetVerifiedAndAddEvent()
    {
        // Arrange
        var user = User.Create(Email.Create("test@example.com"), "John", "Doe");
        user.ClearDomainEvents();

        // Act
        user.VerifyEmail();

        // Assert
        Assert.True(user.IsEmailVerified);
        Assert.Single(user.DomainEvents);
        Assert.IsType<UserEmailVerifiedEvent>(user.DomainEvents.First());
    }

    [Fact]
    public void VerifyEmail_WhenAlreadyVerified_ShouldNotAddEvent()
    {
        // Arrange
        var user = User.Create(Email.Create("test@example.com"), "John", "Doe");
        user.VerifyEmail();
        user.ClearDomainEvents();

        // Act
        user.VerifyEmail();

        // Assert
        Assert.True(user.IsEmailVerified);
        Assert.Empty(user.DomainEvents);
    }

    [Fact]
    public void ChangeEmail_WithDifferentEmail_ShouldUpdateEmailAndAddEvent()
    {
        // Arrange
        var user = User.Create(Email.Create("test@example.com"), "John", "Doe");
        user.VerifyEmail();
        user.ClearDomainEvents();
        var newEmail = Email.Create("newemail@example.com");

        // Act
        user.ChangeEmail(newEmail);

        // Assert
        Assert.Equal(newEmail, user.Email);
        Assert.False(user.IsEmailVerified); // Should require re-verification
        Assert.Single(user.DomainEvents);
        Assert.IsType<UserEmailChangedEvent>(user.DomainEvents.First());
    }

    [Fact]
    public void ChangeEmail_WithSameEmail_ShouldNotChangeAnything()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var user = User.Create(email, "John", "Doe");
        user.VerifyEmail();
        user.ClearDomainEvents();

        // Act
        user.ChangeEmail(email);

        // Assert
        Assert.True(user.IsEmailVerified);
        Assert.Empty(user.DomainEvents);
    }

    [Fact]
    public void UpdateProfile_WithValidData_ShouldUpdateAndAddEvent()
    {
        // Arrange
        var user = User.Create(Email.Create("test@example.com"), "John", "Doe");
        user.ClearDomainEvents();
        var phoneNumber = PhoneNumber.Create("+97312345678");

        // Act
        user.UpdateProfile("Jane", "Smith", phoneNumber);

        // Assert
        Assert.Equal("Jane", user.FirstName);
        Assert.Equal("Smith", user.LastName);
        Assert.Equal(phoneNumber, user.PhoneNumber);
        Assert.Single(user.DomainEvents);
        Assert.IsType<UserProfileUpdatedEvent>(user.DomainEvents.First());
    }

    [Fact]
    public void GetFullName_ShouldReturnConcatenatedName()
    {
        // Arrange
        var user = User.Create(Email.Create("test@example.com"), "John", "Doe");

        // Act
        var fullName = user.GetFullName();

        // Assert
        Assert.Equal("John Doe", fullName);
    }

    [Fact]
    public void CanLogin_WithVerifiedEmailAndPassword_ShouldReturnTrue()
    {
        // Arrange
        var user = User.Create(Email.Create("test@example.com"), "John", "Doe");
        user.VerifyEmail();
        user.SetPassword("hashedpassword");

        // Act
        var canLogin = user.CanLogin();

        // Assert
        Assert.True(canLogin);
    }

    [Fact]
    public void CanLogin_WithGoogleAccount_ShouldReturnTrue()
    {
        // Arrange
        var user = User.CreateWithGoogle(Email.Create("test@example.com"), "John", "Doe", "google123");

        // Act
        var canLogin = user.CanLogin();

        // Assert
        Assert.True(canLogin);
    }

    [Fact]
    public void CanLogin_WithUnverifiedEmail_ShouldReturnFalse()
    {
        // Arrange
        var user = User.Create(Email.Create("test@example.com"), "John", "Doe");
        user.SetPassword("hashedpassword");

        // Act
        var canLogin = user.CanLogin();

        // Assert
        Assert.False(canLogin);
    }
}