using MediatR;

namespace AutismCenter.Application.Features.Authentication.Commands.ResetPassword;

public record ResetPasswordCommand(
    string Token,
    string NewPassword,
    string ConfirmPassword
) : IRequest<ResetPasswordResponse>;