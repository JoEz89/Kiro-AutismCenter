using MediatR;

namespace AutismCenter.Application.Features.Appointments.Commands.UpdateDoctorAvailability;

public record UpdateDoctorAvailabilityCommand(
    Guid DoctorId,
    Guid AvailabilityId,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive
) : IRequest<UpdateDoctorAvailabilityResponse>;