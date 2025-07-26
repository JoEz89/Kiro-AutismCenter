using MediatR;

namespace AutismCenter.Application.Features.Appointments.Commands.RemoveDoctorAvailability;

public record RemoveDoctorAvailabilityCommand(
    Guid DoctorId,
    Guid AvailabilityId
) : IRequest;