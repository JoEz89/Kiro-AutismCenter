namespace AutismCenter.Application.Features.Appointments.Queries.Admin.GetAppointmentAnalytics;

public record GetAppointmentAnalyticsResponse(
    int TotalAppointments,
    int CompletedAppointments,
    int CancelledAppointments,
    int UpcomingAppointments,
    decimal CompletionRate,
    decimal CancellationRate,
    IEnumerable<AppointmentAnalyticsDataPoint> DataPoints,
    IEnumerable<DoctorAppointmentStats> DoctorStats
);

public record AppointmentAnalyticsDataPoint(
    DateTime Date,
    int TotalAppointments,
    int CompletedAppointments,
    int CancelledAppointments,
    string Period
);

public record DoctorAppointmentStats(
    Guid DoctorId,
    string DoctorName,
    int TotalAppointments,
    int CompletedAppointments,
    int CancelledAppointments,
    decimal CompletionRate
);