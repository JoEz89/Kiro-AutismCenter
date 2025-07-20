namespace AutismCenter.Application.Features.Authentication.Commands.VerifyEmail;

public record VerifyEmailResponse(
    bool Success,
    string Message
);