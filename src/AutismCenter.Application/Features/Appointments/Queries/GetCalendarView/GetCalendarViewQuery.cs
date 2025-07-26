using MediatR;

namespace AutismCenter.Application.Features.Appointments.Queries.GetCalendarView;

public record GetCalendarViewQuery(
    DateTime StartDate,
    DateTime EndDate,
    Guid? DoctorId = null
) : IRequest<GetCalendarViewResponse>;