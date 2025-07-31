using MediatR;
using AutismCenter.Domain.Interfaces;

namespace AutismCenter.Application.Features.Appointments.Queries.Admin.GetAppointmentsAdmin;

public class GetAppointmentsAdminHandler : IRequestHandler<GetAppointmentsAdminQuery, GetAppointmentsAdminResponse>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDoctorRepository _doctorRepository;

    public GetAppointmentsAdminHandler(
        IAppointmentRepository appointmentRepository,
        IUserRepository userRepository,
        IDoctorRepository doctorRepository)
    {
        _appointmentRepository = appointmentRepository;
        _userRepository = userRepository;
        _doctorRepository = doctorRepository;
    }

    public async Task<GetAppointmentsAdminResponse> Handle(GetAppointmentsAdminQuery request, CancellationToken cancellationToken)
    {
        // Get all appointments with filtering
        var appointments = await _appointmentRepository.GetAllAsync(cancellationToken);

        // Apply filters
        if (request.Status.HasValue)
        {
            appointments = appointments.Where(a => a.Status == request.Status.Value);
        }

        if (request.DoctorId.HasValue)
        {
            appointments = appointments.Where(a => a.DoctorId == request.DoctorId.Value);
        }

        if (request.StartDate.HasValue)
        {
            appointments = appointments.Where(a => a.AppointmentDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            appointments = appointments.Where(a => a.AppointmentDate <= request.EndDate.Value);
        }

        // Apply search term (search in user email, user name, or doctor name)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            var filteredAppointments = new List<Domain.Entities.Appointment>();

            foreach (var appointment in appointments)
            {
                var user = await _userRepository.GetByIdAsync(appointment.UserId, cancellationToken);
                var doctor = await _doctorRepository.GetByIdAsync(appointment.DoctorId, cancellationToken);

                if (user != null && doctor != null)
                {
                    var userFullName = $"{user.FirstName} {user.LastName}".ToLower();
                    var userEmail = user.Email.Value.ToLower();
                    var doctorName = doctor.NameEn.ToLower();

                    if (userFullName.Contains(searchTerm) || 
                        userEmail.Contains(searchTerm) || 
                        doctorName.Contains(searchTerm))
                    {
                        filteredAppointments.Add(appointment);
                    }
                }
            }

            appointments = filteredAppointments;
        }

        var totalCount = appointments.Count();

        // Apply sorting
        appointments = request.SortBy.ToLower() switch
        {
            "appointmentdate" => request.SortDirection.ToLower() == "asc" 
                ? appointments.OrderBy(a => a.AppointmentDate)
                : appointments.OrderByDescending(a => a.AppointmentDate),
            "status" => request.SortDirection.ToLower() == "asc"
                ? appointments.OrderBy(a => a.Status)
                : appointments.OrderByDescending(a => a.Status),
            "createdat" => request.SortDirection.ToLower() == "asc"
                ? appointments.OrderBy(a => a.CreatedAt)
                : appointments.OrderByDescending(a => a.CreatedAt),
            _ => appointments.OrderByDescending(a => a.AppointmentDate)
        };

        // Apply pagination
        var pagedAppointments = appointments
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Convert to DTOs
        var appointmentDtos = new List<AppointmentAdminDto>();

        foreach (var appointment in pagedAppointments)
        {
            var user = await _userRepository.GetByIdAsync(appointment.UserId, cancellationToken);
            var doctor = await _doctorRepository.GetByIdAsync(appointment.DoctorId, cancellationToken);

            if (user != null && doctor != null)
            {
                appointmentDtos.Add(new AppointmentAdminDto(
                    appointment.Id,
                    appointment.UserId,
                    user.Email.Value,
                    $"{user.FirstName} {user.LastName}",
                    appointment.DoctorId,
                    doctor.NameEn,
                    appointment.AppointmentDate,
                    appointment.Status,
                    appointment.ZoomJoinUrl,
                    appointment.PatientInfo?.CurrentConcerns,
                    appointment.Notes,
                    appointment.CreatedAt,
                    appointment.UpdatedAt
                ));
            }
        }

        return new GetAppointmentsAdminResponse(
            appointmentDtos,
            totalCount,
            request.PageNumber,
            request.PageSize
        );
    }
}