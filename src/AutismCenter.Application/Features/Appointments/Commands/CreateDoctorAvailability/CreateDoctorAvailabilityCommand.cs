using MediatR;

namespace AutismCenter.Application.Features.Appointments.Commands.CreateDoctorAvailability;

public record CreateDoctorAvailabilityCommand(
    Guid DoctorId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime
) : IRequest<CreateDoctorAvailabilityResponse>;