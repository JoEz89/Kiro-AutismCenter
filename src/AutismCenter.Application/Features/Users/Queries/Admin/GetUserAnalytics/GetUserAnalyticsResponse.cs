using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Users.Queries.Admin.GetUserAnalytics;

public record GetUserAnalyticsResponse(
    UserOverviewAnalytics Overview,
    IEnumerable<UserRoleAnalytics> RoleBreakdown,
    IEnumerable<MonthlyUserAnalytics> MonthlyTrends,
    IEnumerable<UserLanguageAnalytics> LanguageBreakdown,
    IEnumerable<UserActivityAnalytics> TopActiveUsers
);

public record UserOverviewAnalytics(
    int TotalUsers,
    int ActiveUsers,
    int InactiveUsers,
    int VerifiedUsers,
    int UnverifiedUsers,
    int GoogleUsers,
    int RegularUsers,
    decimal VerificationRate,
    decimal GoogleAccountRate
);

public record UserRoleAnalytics(
    UserRole Role,
    int Count,
    double Percentage
);

public record MonthlyUserAnalytics(
    int Year,
    int Month,
    int NewUsers,
    int TotalUsers
);

public record UserLanguageAnalytics(
    Language Language,
    int Count,
    double Percentage
);

public record UserActivityAnalytics(
    Guid UserId,
    string FullName,
    string Email,
    UserRole Role,
    int OrderCount,
    int AppointmentCount,
    int EnrollmentCount,
    decimal TotalSpent,
    DateTime LastActivity
);