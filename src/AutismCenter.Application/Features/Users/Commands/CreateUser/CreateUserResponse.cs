namespace AutismCenter.Application.Features.Users.Commands.CreateUser;

public record CreateUserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string PreferredLanguage,
    string? PhoneNumber,
    bool IsEmailVerified,
    DateTime CreatedAt,
    string Message
);