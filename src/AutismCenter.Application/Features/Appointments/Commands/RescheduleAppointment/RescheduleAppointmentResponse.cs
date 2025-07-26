using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Appointments.Commands.RescheduleAppointment;

public record RescheduleAppointmentResponse(
    Guid Id,
    string AppointmentNumber,
    DateTime OldAppointmentDate,
    DateTime NewAppointmentDate,
    AppointmentStatus Status,
    string? ZoomJoinUrl
);