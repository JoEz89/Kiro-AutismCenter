using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Appointments.Commands.BookAppointment;

public record BookAppointmentResponse(
    Guid Id,
    string AppointmentNumber,
    Guid UserId,
    Guid DoctorId,
    string DoctorNameEn,
    string DoctorNameAr,
    DateTime AppointmentDate,
    int DurationInMinutes,
    AppointmentStatus Status,
    string PatientName,
    string? ZoomJoinUrl
);