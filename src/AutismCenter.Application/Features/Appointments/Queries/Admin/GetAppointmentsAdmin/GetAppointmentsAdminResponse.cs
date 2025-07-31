using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Appointments.Queries.Admin.GetAppointmentsAdmin;

public record GetAppointmentsAdminResponse(
    IEnumerable<AppointmentAdminDto> Appointments,
    int TotalCount,
    int PageNumber,
    int PageSize
);

public record AppointmentAdminDto(
    Guid Id,
    Guid UserId,
    string UserEmail,
    string UserFullName,
    Guid DoctorId,
    string DoctorName,
    DateTime AppointmentDate,
    AppointmentStatus Status,
    string? ZoomLink,
    string? PatientNotes,
    string? DoctorNotes,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);