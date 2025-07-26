using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;

namespace AutismCenter.Domain.Services;

public class AppointmentSlotService : IAppointmentSlotService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorRepository _doctorRepository;

    public AppointmentSlotService(IAppointmentRepository appointmentRepository, IDoctorRepository doctorRepository)
    {
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
    }

    public async Task<bool> IsSlotAvailableAsync(Guid doctorId, DateTime startTime, int durationInMinutes, 
        Guid? excludeAppointmentId = null, CancellationToken cancellationToken = default)
    {
        var endTime = startTime.AddMinutes(durationInMinutes);

        // Check if doctor exists and is active
        var doctor = await _doctorRepository.GetByIdAsync(doctorId, cancellationToken);
        if (doctor == null || !doctor.IsActive)
            return false;

        // Check if the time slot is within doctor's availability
        if (!doctor.IsAvailableAt(startTime, durationInMinutes))
            return false;

        // Check for conflicting appointments
        var hasConflict = await _appointmentRepository.HasConflictingAppointmentAsync(
            doctorId, startTime, endTime, excludeAppointmentId, cancellationToken);

        return !hasConflict;
    }

    public async Task<IEnumerable<DateTime>> GenerateAvailableSlotsAsync(Guid doctorId, DateTime date, 
        int durationInMinutes, CancellationToken cancellationToken = default)
    {
        var doctor = await _doctorRepository.GetByIdAsync(doctorId, cancellationToken);
        if (doctor == null || !doctor.IsActive)
            return Enumerable.Empty<DateTime>();

        var dayOfWeek = date.DayOfWeek;
        var availability = doctor.Availability
            .Where(a => a.DayOfWeek == dayOfWeek && a.IsActive)
            .ToList();

        var slots = new List<DateTime>();

        foreach (var avail in availability)
        {
            var slotStart = date.Date.Add(avail.StartTime.ToTimeSpan());
            var availabilityEnd = date.Date.Add(avail.EndTime.ToTimeSpan());

            // Skip past time slots for today
            if (date.Date == DateTime.UtcNow.Date && slotStart <= DateTime.UtcNow.AddMinutes(30))
            {
                var nextValidTime = DateTime.UtcNow.AddMinutes(30);
                var minutesToRound = durationInMinutes - (nextValidTime.Minute % durationInMinutes);
                if (minutesToRound == durationInMinutes) minutesToRound = 0;
                slotStart = nextValidTime.AddMinutes(minutesToRound);
                slotStart = new DateTime(slotStart.Year, slotStart.Month, slotStart.Day, slotStart.Hour, slotStart.Minute, 0);
            }

            while (slotStart.AddMinutes(durationInMinutes) <= availabilityEnd)
            {
                var slotEnd = slotStart.AddMinutes(durationInMinutes);
                
                var hasConflict = await _appointmentRepository.HasConflictingAppointmentAsync(
                    doctorId, slotStart, slotEnd, null, cancellationToken);

                if (!hasConflict)
                {
                    slots.Add(slotStart);
                }

                slotStart = slotStart.AddMinutes(durationInMinutes);
            }
        }

        return slots.OrderBy(s => s);
    }

    public async Task ValidateAppointmentSlotAsync(Guid doctorId, DateTime startTime, int durationInMinutes, 
        Guid? excludeAppointmentId = null, CancellationToken cancellationToken = default)
    {
        if (startTime <= DateTime.UtcNow.AddMinutes(30))
            throw new InvalidOperationException("Appointment must be scheduled at least 30 minutes in advance");

        if (durationInMinutes <= 0)
            throw new ArgumentException("Duration must be positive", nameof(durationInMinutes));

        var isAvailable = await IsSlotAvailableAsync(doctorId, startTime, durationInMinutes, excludeAppointmentId, cancellationToken);
        if (!isAvailable)
            throw new InvalidOperationException("The selected time slot is not available");
    }

    public async Task<bool> CanRescheduleAppointmentAsync(Guid appointmentId, DateTime newStartTime, 
        CancellationToken cancellationToken = default)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment == null)
            return false;

        if (!appointment.CanBeRescheduled())
            return false;

        return await IsSlotAvailableAsync(appointment.DoctorId, newStartTime, 
            appointment.DurationInMinutes, appointmentId, cancellationToken);
    }
}