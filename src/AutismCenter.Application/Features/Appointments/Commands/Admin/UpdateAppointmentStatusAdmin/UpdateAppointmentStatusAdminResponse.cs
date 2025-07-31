using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Appointments.Commands.Admin.UpdateAppointmentStatusAdmin;

public record UpdateAppointmentStatusAdminResponse(
    Guid AppointmentId,
    AppointmentStatus Status,
    string? Notes,
    DateTime UpdatedAt
);