using AutismCenter.Domain.Entities;

namespace AutismCenter.Application.Features.Users.Common;

public record UserProfileDto(
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
    int TotalOrders,
    int TotalEnrollments,
    int TotalAppointments,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public static UserProfileDto FromEntity(User user)
    {
        return new UserProfileDto(
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
            user.Orders.Count,
            user.Enrollments.Count,
            user.Appointments.Count,
            user.CreatedAt,
            user.UpdatedAt
        );
    }
}