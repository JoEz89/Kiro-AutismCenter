using MediatR;

namespace AutismCenter.Application.Features.Appointments.Queries.GetAppointments;

public record GetAppointmentsQuery(
    Guid? UserId = null,
    Guid? DoctorId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    bool UpcomingOnly = false
) : IRequest<GetAppointmentsResponse>;