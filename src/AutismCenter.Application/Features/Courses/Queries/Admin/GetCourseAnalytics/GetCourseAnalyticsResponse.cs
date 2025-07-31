namespace AutismCenter.Application.Features.Courses.Queries.Admin.GetCourseAnalytics;

public record GetCourseAnalyticsResponse(
    CourseOverviewAnalytics Overview,
    IEnumerable<CoursePerformanceAnalytics> TopPerformingCourses,
    IEnumerable<CourseEnrollmentTrend> EnrollmentTrends,
    IEnumerable<CourseCompletionAnalytics> CompletionRates
);

public record CourseOverviewAnalytics(
    int TotalCourses,
    int ActiveCourses,
    int TotalEnrollments,
    int CompletedEnrollments,
    decimal TotalRevenue,
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
    double CompletionRate
);

public record CourseEnrollmentTrend(
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
    double AverageCompletionTimeInDays
);