using MediatR;

namespace AutismCenter.Application.Features.Appointments.Commands.CancelAppointment;

public record CancelAppointmentCommand(Guid AppointmentId, Guid UserId) : IRequest;