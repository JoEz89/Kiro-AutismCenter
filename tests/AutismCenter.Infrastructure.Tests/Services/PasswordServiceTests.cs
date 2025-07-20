using AutismCenter.Infrastructure.Services;
using Xunit;

namespace AutismCenter.Infrastructure.Tests.Services;

public class PasswordServiceTests
{
    private readonly PasswordService _passwordService;

    public PasswordServiceTests()
    {
        _passwordService = new PasswordService();
    }

    [Fact]
    public void HashPassword_ValidPassword_ReturnsHashedPassword()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hashedPassword = _passwordService.HashPassword(password);

        // Assert
        Assert.NotNull(hashedPassword);
        Assert.NotEmpty(hashedPassword);
        Assert.NotEqual(password, hashedPassword);
    }

    [Fact]
    public void HashPassword_EmptyPassword_ThrowsArgumentException()
    {
        // Arrange
        var password = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _passwordService.HashPassword(password));
    }

    [Fact]
    public void HashPassword_NullPassword_ThrowsArgumentException()
    {
        // Arrange
        string password = null!;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _passwordService.HashPassword(password));
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var hashedPassword = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(password, hashedPassword);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_IncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword123!";
        var hashedPassword = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(wrongPassword, hashedPassword);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_EmptyPassword_ReturnsFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var hashedPassword = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword("", hashedPassword);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_EmptyHash_ReturnsFalse()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var result = _passwordService.VerifyPassword(password, "");

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("Password123!", true)]  // Valid: 8+ chars, upper, lower, digit, special
    [InlineData("password123!", false)] // Invalid: no uppercase
    [InlineData("PASSWORD123!", false)] // Invalid: no lowercase
    [InlineData("Password!", false)]    // Invalid: no digit
    [InlineData("Password123", false)]  // Invalid: no special character
    [InlineData("Pass1!", false)]       // Invalid: too short
    [InlineData("", false)]             // Invalid: empty
    [InlineData("ComplexPassword123!", true)] // Valid: complex password
    public void IsValidPassword_VariousPasswords_ReturnsExpectedResult(string password, bool expected)
    {
        // Act
        var result = _passwordService.IsValidPassword(password);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HashPassword_SamePasswordTwice_GeneratesDifferentHashes()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = _passwordService.HashPassword(password);
        var hash2 = _passwordService.HashPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2);
        Assert.True(_passwordService.VerifyPassword(password, hash1));
        Assert.True(_passwordService.VerifyPassword(password, hash2));
    }
}