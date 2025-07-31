namespace AutismCenter.Application.Features.Dashboard.Queries.GetDashboardOverview;

public record GetDashboardOverviewResponse(
    DashboardOverviewMetrics Overview,
    UserMetrics Users,
    ProductMetrics Products,
    OrderMetrics Orders,
    AppointmentMetrics Appointments,
    CourseMetrics Courses,
    RecentActivity RecentActivity
);

public record DashboardOverviewMetrics(
    decimal TotalRevenue,
    int TotalUsers,
    int TotalOrders,
    int TotalAppointments,
    int TotalCourses,
    int TotalProducts,
    double GrowthRate
);

public record UserMetrics(
    int TotalUsers,
    int ActiveUsers,
    int NewUsersThisMonth,
    double UserGrowthRate
);

public record ProductMetrics(
    int TotalProducts,
    int ActiveProducts,
    int LowStockProducts,
    int OutOfStockProducts
);

public record OrderMetrics(
    int TotalOrders,
    decimal TotalRevenue,
    int PendingOrders,
    int CompletedOrders,
    double AverageOrderValue
);

public record AppointmentMetrics(
    int TotalAppointments,
    int UpcomingAppointments,
    int CompletedAppointments,
    int CancelledAppointments,
    double BookingRate
);

public record CourseMetrics(
    int TotalCourses,
    int ActiveCourses,
    int TotalEnrollments,
    int CompletedCourses,
    double CompletionRate
);

public record RecentActivity(
    IEnumerable<ActivityItem> Activities
);

public record ActivityItem(
    string Type,
    string Description,
    DateTime Timestamp,
    string? UserId,
    string? UserName
);