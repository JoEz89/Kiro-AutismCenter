using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Common.Models;
using MediatR;

namespace AutismCenter.Application.Features.Authentication.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthenticationResult>
{
    private readonly IAuthenticationService _authenticationService;

    public LoginCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public async Task<AuthenticationResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _authenticationService.AuthenticateAsync(request.Email, request.Password);
    }
}