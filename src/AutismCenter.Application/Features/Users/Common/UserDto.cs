using AutismCenter.Domain.Entities;

namespace AutismCenter.Application.Features.Users.Common;

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string Role,
    string PreferredLanguage,
    bool IsEmailVerified,
    bool HasGoogleAccount,
    string? PhoneNumber,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public static UserDto FromEntity(User user)
    {
        return new UserDto(
            user.Id,
            user.Email.Value,
            user.FirstName,
            user.LastName,
            user.GetFullName(),
            user.Role.ToString(),
            user.PreferredLanguage == AutismCenter.Domain.Enums.Language.Arabic ? "ar" : "en",
            user.IsEmailVerified,
            user.HasGoogleAccount(),
            user.PhoneNumber?.Value,
            user.CreatedAt,
            user.UpdatedAt
        );
    }
}