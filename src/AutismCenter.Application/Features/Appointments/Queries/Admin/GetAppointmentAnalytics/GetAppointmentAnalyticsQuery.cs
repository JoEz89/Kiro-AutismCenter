using MediatR;

namespace AutismCenter.Application.Features.Appointments.Queries.Admin.GetAppointmentAnalytics;

public record GetAppointmentAnalyticsQuery(
    DateTime? StartDate,
    DateTime? EndDate,
    string GroupBy = "day",
    Guid? DoctorId = null
) : IRequest<GetAppointmentAnalyticsResponse>;