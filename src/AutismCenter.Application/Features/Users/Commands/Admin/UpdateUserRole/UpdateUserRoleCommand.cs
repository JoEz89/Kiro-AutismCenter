using MediatR;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Users.Commands.Admin.UpdateUserRole;

public record UpdateUserRoleCommand(
    Guid UserId,
    UserRole Role
) : IRequest<UpdateUserRoleResponse>;