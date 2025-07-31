using MediatR;

namespace AutismCenter.Application.Features.Appointments.Queries.Admin.ExportAppointments;

public record ExportAppointmentsQuery(
    DateTime? StartDate,
    DateTime? EndDate,
    string? Status,
    Guid? DoctorId,
    string Format
) : IRequest<ExportAppointmentsResponse>;