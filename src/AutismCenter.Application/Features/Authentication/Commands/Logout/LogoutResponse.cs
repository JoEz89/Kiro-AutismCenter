namespace AutismCenter.Application.Features.Authentication.Commands.Logout;

public record LogoutResponse(
    bool Success,
    string Message
);