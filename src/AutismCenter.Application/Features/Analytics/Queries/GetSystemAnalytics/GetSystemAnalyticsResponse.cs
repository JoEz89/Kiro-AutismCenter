using AutismCenter.Application.Features.Users.Queries.Admin.GetUserAnalytics;
using AutismCenter.Application.Features.Products.Queries.Admin.GetProductAnalytics;
using AutismCenter.Application.Features.Orders.Queries.Admin.GetOrderAnalytics;
using AutismCenter.Application.Features.Appointments.Queries.Admin.GetAppointmentAnalytics;

namespace AutismCenter.Application.Features.Analytics.Queries.GetSystemAnalytics;

public record GetSystemAnalyticsResponse(
    SystemOverviewAnalytics Overview,
    GetUserAnalyticsResponse? UserAnalytics,
    GetProductAnalyticsResponse? ProductAnalytics,
    GetOrderAnalyticsResponse? OrderAnalytics,
    GetAppointmentAnalyticsResponse? AppointmentAnalytics,
    GetCourseAnalyticsResponse? CourseAnalytics,
    IEnumerable<CrossModuleAnalytics> CrossModuleInsights
);

public record SystemOverviewAnalytics(
    DateTime AnalyticsPeriodStart,
    DateTime AnalyticsPeriodEnd,
    decimal TotalSystemRevenue,
    int TotalSystemUsers,
    int TotalSystemTransactions,
    double SystemGrowthRate,
    string TopPerformingModule,
    IEnumerable<ModulePerformance> ModulePerformances
);

public record ModulePerformance(
    string ModuleName,
    decimal Revenue,
    int Transactions,
    double GrowthRate,
    string Status
);

public record CrossModuleAnalytics(
    string InsightType,
    string Description,
    decimal Value,
    string Trend,
    IEnumerable<string> AffectedModules
);

public record GetCourseAnalyticsResponse(
    CourseOverviewAnalytics Overview,
    IEnumerable<CoursePerformanceAnalytics> TopCourses,
    IEnumerable<CourseEnrollmentAnalytics> EnrollmentTrends,
    IEnumerable<CourseCompletionAnalytics> CompletionRates
);

public record CourseOverviewAnalytics(
    int TotalCourses,
    int ActiveCourses,
    int TotalEnrollments,
    int CompletedEnrollments,
    decimal TotalCourseRevenue,
    double AverageCompletionRate,
    double AverageEnrollmentDuration
);

public record CoursePerformanceAnalytics(
    Guid CourseId,
    string CourseTitleEn,
    string CourseTitleAr,
    int EnrollmentCount,
    int CompletionCount,
    decimal Revenue,
    double CompletionRate,
    double AverageRating
);

public record CourseEnrollmentAnalytics(
    DateTime Date,
    int EnrollmentCount,
    decimal Revenue
);

public record CourseCompletionAnalytics(
    Guid CourseId,
    string CourseTitleEn,
    int TotalEnrollments,
    int CompletedEnrollments,
    double CompletionRate,
    double AverageCompletionTime
);