using MediatR;

namespace AutismCenter.Application.Features.Authentication.Commands.Logout;

public record LogoutCommand(
    string RefreshToken
) : IRequest<LogoutResponse>;