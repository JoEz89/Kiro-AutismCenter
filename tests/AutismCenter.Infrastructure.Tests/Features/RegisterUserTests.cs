using AutismCenter.Application.Features.Authentication.Commands.RegisterUser;
using AutismCenter.Infrastructure.Services;
using Xunit;

namespace AutismCenter.Infrastructure.Tests.Features;

public class RegisterUserTests
{
    [Fact]
    public void RegisterUserValidator_ValidCommand_PassesValidation()
    {
        // Arrange
        var validator = new RegisterUserValidator();
        var command = new RegisterUserCommand(
            "test@example.com",
            "John",
            "Doe",
            "TestPassword123!",
            "TestPassword123!",
            "en"
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void RegisterUserValidator_InvalidEmail_FailsValidation()
    {
        // Arrange
        var validator = new RegisterUserValidator();
        var command = new RegisterUserCommand(
            "invalid-email",
            "John",
            "Doe",
            "TestPassword123!",
            "TestPassword123!",
            "en"
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void RegisterUserValidator_WeakPassword_FailsValidation()
    {
        // Arrange
        var validator = new RegisterUserValidator();
        var command = new RegisterUserCommand(
            "test@example.com",
            "John",
            "Doe",
            "weak",
            "weak",
            "en"
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Fact]
    public void RegisterUserValidator_PasswordMismatch_FailsValidation()
    {
        // Arrange
        var validator = new RegisterUserValidator();
        var command = new RegisterUserCommand(
            "test@example.com",
            "John",
            "Doe",
            "TestPassword123!",
            "DifferentPassword123!",
            "en"
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ConfirmPassword");
    }
}