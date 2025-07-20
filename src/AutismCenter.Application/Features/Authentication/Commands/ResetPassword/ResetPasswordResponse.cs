namespace AutismCenter.Application.Features.Authentication.Commands.ResetPassword;

public record ResetPasswordResponse(
    bool Success,
    string Message
);