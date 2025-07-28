using AutismCenter.Application.Common.Models;
using MediatR;

namespace AutismCenter.Application.Features.Authentication.Commands.RefreshToken;

public record RefreshTokenCommand(
    string RefreshToken
) : IRequest<AuthenticationResult>;