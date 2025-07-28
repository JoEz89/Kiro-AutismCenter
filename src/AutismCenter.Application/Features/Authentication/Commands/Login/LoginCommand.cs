using AutismCenter.Application.Common.Models;
using MediatR;

namespace AutismCenter.Application.Features.Authentication.Commands.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<AuthenticationResult>;