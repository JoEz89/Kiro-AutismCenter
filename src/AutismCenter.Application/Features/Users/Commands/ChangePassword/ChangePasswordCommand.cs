using MediatR;

namespace AutismCenter.Application.Features.Users.Commands.ChangePassword;

public record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
) : IRequest<ChangePasswordResponse>;