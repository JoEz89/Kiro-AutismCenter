namespace AutismCenter.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string PreferredLanguage,
    DateTime UpdatedAt,
    string Message
);