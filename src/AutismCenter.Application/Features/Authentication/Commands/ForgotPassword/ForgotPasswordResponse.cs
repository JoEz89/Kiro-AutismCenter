namespace AutismCenter.Application.Features.Authentication.Commands.ForgotPassword;

public record ForgotPasswordResponse(
    bool Success,
    string Message
);