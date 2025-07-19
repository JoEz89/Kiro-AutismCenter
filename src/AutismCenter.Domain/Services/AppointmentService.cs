using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;

namespace AutismCenter.Domain.Services;

public class AppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorRepository _doctorRepository;

    public AppointmentService(IAppointmentRepository appointmentRepository, IDoctorRepository doctorRepository)
    {
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
    }

    public async Task<Appointment> ScheduleAppointmentAsync(Guid userId, Guid doctorId, DateTime appointmentDate, 
        int durationInMinutes, PatientInfo patientInfo, CancellationToken cancellationToken = default)
    {
        // Validate doctor exists and is active
        var doctor = await _doctorRepository.GetByIdAsync(doctorId, cancellationToken);
        if (doctor == null)
            throw new InvalidOperationException("Doctor not found");

        if (!doctor.IsActive)
            throw new InvalidOperationException("Doctor is not active");

        // Check if doctor is available at the requested time
        if (!doctor.IsAvailableAt(appointmentDate, durationInMinutes))
            throw new InvalidOperationException("Doctor is not available at the requested time");

        // Check for conflicting appointments
        var endTime = appointmentDate.AddMinutes(durationInMinutes);
        var hasConflict = await _appointmentRepository.HasConflictingAppointmentAsync(
            doctorId, appointmentDate, endTime, cancellationToken: cancellationToken);

        if (hasConflict)
            throw new InvalidOperationException("Doctor already has an appointment at the requested time");

        // Generate appointment number
        var appointmentNumber = await _appointmentRepository.GenerateAppointmentNumberAsync(cancellationToken);

        // Create appointment
        var appointment = Appointment.Create(userId, doctorId, appointmentDate, durationInMinutes, 
            appointmentNumber, patientInfo);

        return appointment;
    }

    public async Task<bool> CanRescheduleAppointmentAsync(Guid appointmentId, DateTime newDateTime, 
        CancellationToken cancellationToken = default)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment == null)
            return false;

        if (!appointment.CanBeRescheduled())
            return false;

        var doctor = await _doctorRepository.GetByIdAsync(appointment.DoctorId, cancellationToken);
        if (doctor == null || !doctor.IsActive)
            return false;

        if (!doctor.IsAvailableAt(newDateTime, appointment.DurationInMinutes))
            return false;

        var endTime = newDateTime.AddMinutes(appointment.DurationInMinutes);
        var hasConflict = await _appointmentRepository.HasConflictingAppointmentAsync(
            appointment.DoctorId, newDateTime, endTime, appointmentId, cancellationToken);

        return !hasConflict;
    }

    public async Task<IEnumerable<DateTime>> GetAvailableSlotsAsync(Guid doctorId, DateTime date, 
        int durationInMinutes, CancellationToken cancellationToken = default)
    {
        var doctor = await _doctorRepository.GetByIdAsync(doctorId, cancellationToken);
        if (doctor == null || !doctor.IsActive)
            return Enumerable.Empty<DateTime>();

        var availableSlots = new List<DateTime>();
        var dayOfWeek = date.DayOfWeek;

        // Get doctor's availability for the day
        var availability = doctor.Availability.FirstOrDefault(a => a.DayOfWeek == dayOfWeek && a.IsActive);
        if (availability == null)
            return availableSlots;

        // Generate time slots
        var startTime = date.Date.Add(availability.StartTime.ToTimeSpan());
        var endTime = date.Date.Add(availability.EndTime.ToTimeSpan());

        var currentSlot = startTime;
        while (currentSlot.AddMinutes(durationInMinutes) <= endTime)
        {
            var slotEndTime = currentSlot.AddMinutes(durationInMinutes);
            
            // Check if slot is available (no conflicting appointments)
            var hasConflict = await _appointmentRepository.HasConflictingAppointmentAsync(
                doctorId, currentSlot, slotEndTime, cancellationToken: cancellationToken);

            if (!hasConflict)
            {
                availableSlots.Add(currentSlot);
            }

            currentSlot = currentSlot.AddMinutes(30); // 30-minute intervals
        }

        return availableSlots;
    }

    public bool IsAppointmentTimeValid(DateTime appointmentDate)
    {
        // Appointment must be in the future
        if (appointmentDate <= DateTime.UtcNow)
            return false;

        // Appointment must be within business hours (8 AM to 6 PM)
        var timeOfDay = appointmentDate.TimeOfDay;
        if (timeOfDay < TimeSpan.FromHours(8) || timeOfDay > TimeSpan.FromHours(18))
            return false;

        // No appointments on weekends (for now)
        if (appointmentDate.DayOfWeek == DayOfWeek.Friday || appointmentDate.DayOfWeek == DayOfWeek.Saturday)
            return false;

        return true;
    }
}