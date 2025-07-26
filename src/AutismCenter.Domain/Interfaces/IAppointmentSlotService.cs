namespace AutismCenter.Domain.Interfaces;

public interface IAppointmentSlotService
{
    Task<bool> IsSlotAvailableAsync(Guid doctorId, DateTime startTime, int durationInMinutes, 
        Guid? excludeAppointmentId = null, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<DateTime>> GenerateAvailableSlotsAsync(Guid doctorId, DateTime date, 
        int durationInMinutes, CancellationToken cancellationToken = default);
    
    Task ValidateAppointmentSlotAsync(Guid doctorId, DateTime startTime, int durationInMinutes, 
        Guid? excludeAppointmentId = null, CancellationToken cancellationToken = default);
    
    Task<bool> CanRescheduleAppointmentAsync(Guid appointmentId, DateTime newStartTime, 
        CancellationToken cancellationToken = default);
}