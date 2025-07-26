namespace AutismCenter.Application.Features.Appointments.Queries.GetDoctorAvailability;

public record GetDoctorAvailabilityResponse(
    Guid DoctorId,
    string DoctorNameEn,
    string DoctorNameAr,
    IEnumerable<DoctorAvailabilityDto> Availability
);

public record DoctorAvailabilityDto(
    Guid Id,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive
);