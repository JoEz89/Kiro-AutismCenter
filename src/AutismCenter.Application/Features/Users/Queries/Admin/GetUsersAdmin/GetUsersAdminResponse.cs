using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Users.Queries.Admin.GetUsersAdmin;

public record GetUsersAdminResponse(
    IEnumerable<UserAdminDto> Users,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);

public record UserAdminDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    UserRole Role,
    Language PreferredLanguage,
    bool IsActive,
    bool IsEmailVerified,
    bool HasGoogleAccount,
    string? PhoneNumber,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int OrderCount,
    int AppointmentCount,
    int EnrollmentCount
);