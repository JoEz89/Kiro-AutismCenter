using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Appointments.Queries.GetCalendarView;

public record GetCalendarViewResponse(
    DateTime StartDate,
    DateTime EndDate,
    IEnumerable<CalendarDayDto> Days
);

public record CalendarDayDto(
    DateTime Date,
    IEnumerable<CalendarAppointmentDto> Appointments,
    IEnumerable<CalendarAvailabilityDto> Availability
);

public record CalendarAppointmentDto(
    Guid Id,
    string AppointmentNumber,
    Guid UserId,
    string PatientName,
    Guid DoctorId,
    string DoctorNameEn,
    string DoctorNameAr,
    DateTime StartTime,
    DateTime EndTime,
    AppointmentStatus Status,
    bool HasZoomMeeting
);

public record CalendarAvailabilityDto(
    Guid DoctorId,
    string DoctorNameEn,
    string DoctorNameAr,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive
);