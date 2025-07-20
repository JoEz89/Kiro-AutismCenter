using AutismCenter.Application.Features.Authentication.Commands.ForgotPassword;
using AutismCenter.Application.Features.Authentication.Commands.ResetPassword;
using Xunit;

namespace AutismCenter.Infrastructure.Tests.Features;

public class PasswordResetTests
{
    [Fact]
    public void ForgotPasswordCommand_ValidEmail_CreatesCommand()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var command = new ForgotPasswordCommand(email);

        // Assert
        Assert.Equal(email, command.Email);
    }

    [Fact]
    public void ResetPasswordCommand_ValidData_CreatesCommand()
    {
        // Arrange
        var token = "reset-token";
        var password = "NewPassword123!";
        var confirmPassword = "NewPassword123!";

        // Act
        var command = new ResetPasswordCommand(token, password, confirmPassword);

        // Assert
        Assert.Equal(token, command.Token);
        Assert.Equal(password, command.NewPassword);
        Assert.Equal(confirmPassword, command.ConfirmPassword);
    }

    [Fact]
    public void ResetPasswordValidator_ValidCommand_PassesValidation()
    {
        // Arrange
        var validator = new ResetPasswordValidator();
        var command = new ResetPasswordCommand(
            "valid-token",
            "NewPassword123!",
            "NewPassword123!"
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ResetPasswordValidator_WeakPassword_FailsValidation()
    {
        // Arrange
        var validator = new ResetPasswordValidator();
        var command = new ResetPasswordCommand(
            "valid-token",
            "weak",
            "weak"
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "NewPassword");
    }

    [Fact]
    public void ResetPasswordValidator_PasswordMismatch_FailsValidation()
    {
        // Arrange
        var validator = new ResetPasswordValidator();
        var command = new ResetPasswordCommand(
            "valid-token",
            "NewPassword123!",
            "DifferentPassword123!"
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ConfirmPassword");
    }

    [Fact]
    public void ResetPasswordValidator_EmptyToken_FailsValidation()
    {
        // Arrange
        var validator = new ResetPasswordValidator();
        var command = new ResetPasswordCommand(
            "",
            "NewPassword123!",
            "NewPassword123!"
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Token");
    }
}