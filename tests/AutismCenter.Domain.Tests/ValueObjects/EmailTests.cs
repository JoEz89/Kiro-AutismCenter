using AutismCenter.Domain.ValueObjects;

namespace AutismCenter.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_ShouldReturnEmail()
    {
        // Arrange
        var emailString = "test@example.com";

        // Act
        var email = Email.Create(emailString);

        // Assert
        Assert.Equal(emailString, email.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyEmail_ShouldThrowArgumentException(string emailString)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Email.Create(emailString));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test.example.com")]
    public void Create_WithInvalidEmail_ShouldThrowArgumentException(string emailString)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Email.Create(emailString));
    }

    [Fact]
    public void Create_WithValidEmail_ShouldNormalizeToLowerCase()
    {
        // Arrange
        var emailString = "TEST@EXAMPLE.COM";

        // Act
        var email = Email.Create(emailString);

        // Assert
        Assert.Equal("test@example.com", email.Value);
    }

    [Fact]
    public void Equals_WithSameEmail_ShouldReturnTrue()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");

        // Act & Assert
        Assert.Equal(email1, email2);
    }

    [Fact]
    public void Equals_WithDifferentEmail_ShouldReturnFalse()
    {
        // Arrange
        var email1 = Email.Create("test1@example.com");
        var email2 = Email.Create("test2@example.com");

        // Act & Assert
        Assert.NotEqual(email1, email2);
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldReturnValue()
    {
        // Arrange
        var email = Email.Create("test@example.com");

        // Act
        string emailString = email;

        // Assert
        Assert.Equal("test@example.com", emailString);
    }
}