using MediatR;

namespace AutismCenter.Application.Features.Users.Commands.Admin.ActivateUser;

public record ActivateUserCommand(
    Guid UserId
) : IRequest<ActivateUserResponse>;