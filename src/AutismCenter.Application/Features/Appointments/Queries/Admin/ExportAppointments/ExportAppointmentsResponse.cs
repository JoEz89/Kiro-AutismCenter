namespace AutismCenter.Application.Features.Appointments.Queries.Admin.ExportAppointments;

public record ExportAppointmentsResponse(
    byte[] FileContent,
    string ContentType,
    string FileName
);