namespace AutismCenter.Application.Features.Appointments.Commands.UpdateDoctorAvailability;

public record UpdateDoctorAvailabilityResponse(
    Guid Id,
    Guid DoctorId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive
);