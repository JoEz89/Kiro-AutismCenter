namespace AutismCenter.Application.Features.Authentication.Commands.RegisterUser;

public record RegisterUserResponse(
    Guid UserId,
    string Email,
    string Message
);