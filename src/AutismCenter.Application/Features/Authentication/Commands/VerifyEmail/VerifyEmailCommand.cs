using MediatR;

namespace AutismCenter.Application.Features.Authentication.Commands.VerifyEmail;

public record VerifyEmailCommand(string Token) : IRequest<VerifyEmailResponse>;