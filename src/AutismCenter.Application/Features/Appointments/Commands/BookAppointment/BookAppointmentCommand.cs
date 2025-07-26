using MediatR;

namespace AutismCenter.Application.Features.Appointments.Commands.BookAppointment;

public record BookAppointmentCommand(
    Guid UserId,
    Guid DoctorId,
    DateTime AppointmentDate,
    int DurationInMinutes,
    string PatientName,
    int PatientAge,
    string? MedicalHistory = null,
    string? CurrentConcerns = null,
    string? EmergencyContact = null,
    string? EmergencyPhone = null
) : IRequest<BookAppointmentResponse>;