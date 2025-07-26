namespace AutismCenter.Application.Features.Appointments.Commands.CreateDoctorAvailability;

public record CreateDoctorAvailabilityResponse(
    Guid Id,
    Guid DoctorId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive
);