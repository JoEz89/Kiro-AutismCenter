using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Appointments.Queries.GetAppointments;

public record GetAppointmentsResponse(
    IEnumerable<AppointmentDto> Appointments
);

public record AppointmentDto(
    Guid Id,
    string AppointmentNumber,
    Guid UserId,
    Guid DoctorId,
    string DoctorNameEn,
    string DoctorNameAr,
    string DoctorSpecialtyEn,
    string DoctorSpecialtyAr,
    DateTime AppointmentDate,
    DateTime EndTime,
    int DurationInMinutes,
    AppointmentStatus Status,
    string PatientName,
    int PatientAge,
    string? MedicalHistory,
    string? CurrentConcerns,
    string? EmergencyContact,
    string? EmergencyPhone,
    string? ZoomMeetingId,
    string? ZoomJoinUrl,
    string? Notes,
    DateTime CreatedAt
);