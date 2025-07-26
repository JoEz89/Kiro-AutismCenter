using MediatR;

namespace AutismCenter.Application.Features.Appointments.Commands.RescheduleAppointment;

public record RescheduleAppointmentCommand(
    Guid AppointmentId,
    Guid UserId,
    DateTime NewAppointmentDate
) : IRequest<RescheduleAppointmentResponse>;