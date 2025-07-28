namespace AutismCenter.Application.Features.Users.Commands.Admin.DeactivateUser;

public record DeactivateUserResponse(
    Guid UserId,
    string Email,
    string FullName,
    bool IsActive,
    DateTime UpdatedAt
);