using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Common.Models;
using MediatR;

namespace AutismCenter.Application.Features.Authentication.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthenticationResult>
{
    private readonly IAuthenticationService _authenticationService;

    public RefreshTokenCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public async Task<AuthenticationResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await _authenticationService.RefreshTokenAsync(request.RefreshToken);
    }
}