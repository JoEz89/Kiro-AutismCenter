namespace AutismCenter.Application.Features.Appointments.Queries.GetAvailableSlots;

public record GetAvailableSlotsResponse(
    IEnumerable<DoctorSlotsDto> DoctorSlots
);

public record DoctorSlotsDto(
    Guid DoctorId,
    string DoctorNameEn,
    string DoctorNameAr,
    string SpecialtyEn,
    string SpecialtyAr,
    IEnumerable<AppointmentSlotDto> AvailableSlots
);

public record AppointmentSlotDto(
    DateTime StartTime,
    DateTime EndTime,
    int DurationInMinutes,
    bool IsAvailable
);