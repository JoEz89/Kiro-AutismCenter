using MediatR;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Appointments.Queries.Admin.GetAppointmentsAdmin;

public record GetAppointmentsAdminQuery(
    int PageNumber = 1,
    int PageSize = 20,
    AppointmentStatus? Status = null,
    Guid? DoctorId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? SearchTerm = null,
    string SortBy = "AppointmentDate",
    string SortDirection = "desc"
) : IRequest<GetAppointmentsAdminResponse>;