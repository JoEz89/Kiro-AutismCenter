using MediatR;

namespace AutismCenter.Application.Features.Appointments.Queries.GetAvailableSlots;

public record GetAvailableSlotsQuery(
    Guid? DoctorId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int DurationInMinutes = 60
) : IRequest<GetAvailableSlotsResponse>;