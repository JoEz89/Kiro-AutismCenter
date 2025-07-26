using AutismCenter.Domain.Entities;

namespace AutismCenter.Domain.Interfaces;

public interface IAppointmentNotificationService
{
    Task SendAppointmentConfirmationAsync(Appointment appointment, CancellationToken cancellationToken = default);
    Task SendAppointmentReminderAsync(Appointment appointment, CancellationToken cancellationToken = default);
    Task SendAppointmentCancellationAsync(Appointment appointment, CancellationToken cancellationToken = default);
    Task SendAppointmentRescheduledAsync(Appointment appointment, DateTime oldDate, CancellationToken cancellationToken = default);
    Task SendAppointmentStartingSoonAsync(Appointment appointment, CancellationToken cancellationToken = default);
}