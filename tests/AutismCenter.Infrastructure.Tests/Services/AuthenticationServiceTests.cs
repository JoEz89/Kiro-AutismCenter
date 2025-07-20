using AutismCenter.Infrastructure.Services;
using Xunit;

namespace AutismCenter.Infrastructure.Tests.Services;

public class AuthenticationServiceTests
{
    [Fact]
    public void PasswordService_HashPassword_WorksCorrectly()
    {
        // Arrange
        var passwordService = new PasswordService();
        var password = "TestPassword123!";

        // Act
        var hashedPassword = passwordService.HashPassword(password);

        // Assert
        Assert.NotNull(hashedPassword);
        Assert.NotEmpty(hashedPassword);
        Assert.True(passwordService.VerifyPassword(password, hashedPassword));
    }
}