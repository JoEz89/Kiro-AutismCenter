using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Appointments.Queries.GetCalendarView;

public class GetCalendarViewHandler : IRequestHandler<GetCalendarViewQuery, GetCalendarViewResponse>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorRepository _doctorRepository;

    public GetCalendarViewHandler(IAppointmentRepository appointmentRepository, IDoctorRepository doctorRepository)
    {
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
    }

    public async Task<GetCalendarViewResponse> Handle(GetCalendarViewQuery request, CancellationToken cancellationToken)
    {
        // Get appointments in the date range
        var appointments = await _appointmentRepository.GetByDateRangeAsync(
            request.StartDate, request.EndDate, cancellationToken);

        // Filter by doctor if specified
        if (request.DoctorId.HasValue)
        {
            appointments = appointments.Where(a => a.DoctorId == request.DoctorId.Value);
        }

        // Get doctors for availability
        var doctors = request.DoctorId.HasValue 
            ? new[] { await _doctorRepository.GetByIdAsync(request.DoctorId.Value, cancellationToken) }.Where(d => d != null)
            : await _doctorRepository.GetActiveAsync(cancellationToken);

        var days = new List<CalendarDayDto>();
        var currentDate = request.StartDate.Date;

        while (currentDate <= request.EndDate.Date)
        {
            // Get appointments for this day
            var dayAppointments = appointments
                .Where(a => a.AppointmentDate.Date == currentDate)
                .Select(a => new CalendarAppointmentDto(
                    a.Id,
                    a.AppointmentNumber,
                    a.UserId,
                    a.PatientInfo.PatientName,
                    a.DoctorId,
                    a.Doctor?.NameEn ?? "",
                    a.Doctor?.NameAr ?? "",
                    a.AppointmentDate,
                    a.GetEndTime(),
                    a.Status,
                    a.HasZoomMeeting()))
                .OrderBy(a => a.StartTime)
                .ToList();

            // Get availability for this day
            var dayOfWeek = currentDate.DayOfWeek;
            var dayAvailability = new List<CalendarAvailabilityDto>();

            foreach (var doctor in doctors)
            {
                if (doctor == null) continue;

                var doctorAvailability = doctor.Availability
                    .Where(a => a.DayOfWeek == dayOfWeek && a.IsActive)
                    .Select(a => new CalendarAvailabilityDto(
                        doctor.Id,
                        doctor.NameEn,
                        doctor.NameAr,
                        a.StartTime,
                        a.EndTime,
                        a.IsActive))
                    .ToList();

                dayAvailability.AddRange(doctorAvailability);
            }

            days.Add(new CalendarDayDto(
                currentDate,
                dayAppointments,
                dayAvailability.OrderBy(a => a.StartTime)));

            currentDate = currentDate.AddDays(1);
        }

        return new GetCalendarViewResponse(
            request.StartDate,
            request.EndDate,
            days);
    }
}