namespace AutismCenter.Application.Features.Users.Commands.Admin.ActivateUser;

public record ActivateUserResponse(
    Guid UserId,
    string Email,
    string FullName,
    bool IsActive,
    DateTime UpdatedAt
);