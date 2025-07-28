using AutismCenter.Application.Common.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Authentication.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, LogoutResponse>
{
    private readonly IAuthenticationService _authenticationService;

    public LogoutCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public async Task<LogoutResponse> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _authenticationService.RevokeTokenAsync(request.RefreshToken);
            return new LogoutResponse(true, "Successfully logged out");
        }
        catch (Exception ex)
        {
            return new LogoutResponse(false, $"Logout failed: {ex.Message}");
        }
    }
}