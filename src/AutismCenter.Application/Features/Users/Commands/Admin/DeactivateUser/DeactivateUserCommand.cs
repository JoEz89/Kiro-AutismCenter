using MediatR;

namespace AutismCenter.Application.Features.Users.Commands.Admin.DeactivateUser;

public record DeactivateUserCommand(
    Guid UserId,
    string? Reason = null
) : IRequest<DeactivateUserResponse>;