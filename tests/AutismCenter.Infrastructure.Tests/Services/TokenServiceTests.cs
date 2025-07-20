using AutismCenter.Application.Common.Settings;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace AutismCenter.Infrastructure.Tests.Services;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public TokenServiceTests()
    {
        _jwtSettings = new JwtSettings
        {
            SecretKey = "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };

        var options = Options.Create(_jwtSettings);
        _tokenService = new TokenService(options);
    }

    [Fact]
    public void GenerateAccessToken_ValidUser_ReturnsToken()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _tokenService.GenerateAccessToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Contains(".", token); // JWT tokens contain dots
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsUniqueTokens()
    {
        // Act
        var token1 = _tokenService.GenerateRefreshToken();
        var token2 = _tokenService.GenerateRefreshToken();

        // Assert
        Assert.NotNull(token1);
        Assert.NotNull(token2);
        Assert.NotEqual(token1, token2);
        Assert.NotEmpty(token1);
        Assert.NotEmpty(token2);
    }

    [Fact]
    public void ValidateToken_ValidToken_ReturnsValidResult()
    {
        // Arrange
        var user = CreateTestUser();
        var token = _tokenService.GenerateAccessToken(user);

        // Act
        var result = _tokenService.ValidateToken(token);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(user.Email.Value, result.Email);
        Assert.Equal(user.Role.ToString(), result.Role);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ValidateToken_InvalidToken_ReturnsInvalidResult()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var result = _tokenService.ValidateToken(invalidToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Null(result.UserId);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void ValidateToken_EmptyToken_ReturnsInvalidResult()
    {
        // Act
        var result = _tokenService.ValidateToken("");

        // Assert
        Assert.False(result.IsValid);
        Assert.Null(result.UserId);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void GetTokenExpiration_ValidToken_ReturnsCorrectExpiration()
    {
        // Arrange
        var user = CreateTestUser();
        var token = _tokenService.GenerateAccessToken(user);
        var expectedExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        // Act
        var expiration = _tokenService.GetTokenExpiration(token);

        // Assert
        Assert.True(expiration > DateTime.UtcNow);
        Assert.True(Math.Abs((expiration - expectedExpiration).TotalMinutes) < 1); // Within 1 minute tolerance
    }

    [Fact]
    public void GetTokenExpiration_InvalidToken_ReturnsMinValue()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var expiration = _tokenService.GetTokenExpiration(invalidToken);

        // Assert
        Assert.Equal(DateTime.MinValue, expiration);
    }

    [Fact]
    public void GenerateAccessToken_ContainsExpectedClaims()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _tokenService.GenerateAccessToken(user);
        var validationResult = _tokenService.ValidateToken(token);

        // Assert
        Assert.True(validationResult.IsValid);
        Assert.Equal(user.Id, validationResult.UserId);
        Assert.Equal(user.Email.Value, validationResult.Email);
        Assert.Equal(user.Role.ToString(), validationResult.Role);
    }

    private static User CreateTestUser()
    {
        var email = Email.Create("test@example.com");
        var user = User.Create(email, "John", "Doe");
        user.VerifyEmail();
        return user;
    }
}