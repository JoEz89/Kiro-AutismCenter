using AutismCenter.Application.Common.Models;
using MediatR;

namespace AutismCenter.Application.Features.Authentication.Commands.GoogleLogin;

public record GoogleLoginCommand(string GoogleToken) : IRequest<AuthenticationResult>;