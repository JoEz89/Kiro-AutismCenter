using MediatR;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Appointments.Commands.Admin.UpdateAppointmentStatusAdmin;

public record UpdateAppointmentStatusAdminCommand(
    Guid AppointmentId,
    AppointmentStatus Status,
    string? Notes = null
) : IRequest<UpdateAppointmentStatusAdminResponse>;