using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Users.Commands.Admin.UpdateUserRole;

public record UpdateUserRoleResponse(
    Guid UserId,
    string Email,
    string FullName,
    UserRole Role,
    DateTime UpdatedAt
);