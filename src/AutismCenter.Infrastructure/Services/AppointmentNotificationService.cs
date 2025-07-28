using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AutismCenter.Infrastructure.Services;

public class AppointmentNotificationService : IAppointmentNotificationService
{
    private readonly IEmailService _emailService;
    private readonly ILogger<AppointmentNotificationService> _logger;

    public AppointmentNotificationService(IEmailService emailService, ILogger<AppointmentNotificationService> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task SendAppointmentConfirmationAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = "Appointment Confirmation - Autism Center";
            var body = GenerateConfirmationEmailBody(appointment);

            await _emailService.SendEmailAsync(
                appointment.User.Email.Value,
                subject,
                body,
                cancellationToken);

            _logger.LogInformation("Appointment confirmation email sent for appointment {AppointmentId}", appointment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send appointment confirmation email for appointment {AppointmentId}", appointment.Id);
            throw;
        }
    }

    public async Task SendAppointmentReminderAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = "Appointment Reminder - Autism Center";
            var body = GenerateReminderEmailBody(appointment);

            await _emailService.SendEmailAsync(
                appointment.User.Email.Value,
                subject,
                body,
                cancellationToken);

            _logger.LogInformation("Appointment reminder email sent for appointment {AppointmentId}", appointment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send appointment reminder email for appointment {AppointmentId}", appointment.Id);
            throw;
        }
    }

    public async Task SendAppointmentCancellationAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = "Appointment Cancelled - Autism Center";
            var body = GenerateCancellationEmailBody(appointment);

            await _emailService.SendEmailAsync(
                appointment.User.Email.Value,
                subject,
                body,
                cancellationToken);

            _logger.LogInformation("Appointment cancellation email sent for appointment {AppointmentId}", appointment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send appointment cancellation email for appointment {AppointmentId}", appointment.Id);
            throw;
        }
    }

    public async Task SendAppointmentRescheduledAsync(Appointment appointment, DateTime oldDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = "Appointment Rescheduled - Autism Center";
            var body = GenerateRescheduledEmailBody(appointment, oldDate);

            await _emailService.SendEmailAsync(
                appointment.User.Email.Value,
                subject,
                body,
                cancellationToken);

            _logger.LogInformation("Appointment rescheduled email sent for appointment {AppointmentId}", appointment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send appointment rescheduled email for appointment {AppointmentId}", appointment.Id);
            throw;
        }
    }

    public async Task SendAppointmentStartingSoonAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = "Your Appointment Starts Soon - Autism Center";
            var body = GenerateStartingSoonEmailBody(appointment);

            await _emailService.SendEmailAsync(
                appointment.User.Email.Value,
                subject,
                body,
                cancellationToken);

            _logger.LogInformation("Appointment starting soon email sent for appointment {AppointmentId}", appointment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send appointment starting soon email for appointment {AppointmentId}", appointment.Id);
            throw;
        }
    }

    private static string GenerateConfirmationEmailBody(Appointment appointment)
    {
        return $@"
Dear {appointment.User.FirstName} {appointment.User.LastName},

Your appointment has been confirmed with the following details:

Appointment Number: {appointment.AppointmentNumber}
Patient: {appointment.PatientInfo.PatientName}
Doctor: {appointment.Doctor?.NameEn}
Date & Time: {appointment.AppointmentDate:dddd, MMMM dd, yyyy 'at' h:mm tt}
Duration: {appointment.DurationInMinutes} minutes

{(appointment.HasZoomMeeting() ? $"Zoom Meeting Link: {appointment.ZoomJoinUrl}" : "")}

If you need to cancel or reschedule this appointment, please do so at least 24 hours in advance.

Thank you for choosing Autism Center.

Best regards,
Autism Center Team
";
    }

    private static string GenerateReminderEmailBody(Appointment appointment)
    {
        var timeUntilAppointment = appointment.AppointmentDate - DateTime.UtcNow;
        var reminderText = timeUntilAppointment.TotalHours <= 1 
            ? "in less than 1 hour" 
            : $"in {Math.Ceiling(timeUntilAppointment.TotalHours)} hours";

        return $@"
Dear {appointment.User.FirstName} {appointment.User.LastName},

This is a reminder that you have an appointment {reminderText}:

Appointment Number: {appointment.AppointmentNumber}
Patient: {appointment.PatientInfo.PatientName}
Doctor: {appointment.Doctor?.NameEn}
Date & Time: {appointment.AppointmentDate:dddd, MMMM dd, yyyy 'at' h:mm tt}
Duration: {appointment.DurationInMinutes} minutes

{(appointment.HasZoomMeeting() ? $"Zoom Meeting Link: {appointment.ZoomJoinUrl}" : "")}

Please make sure to join the meeting on time.

Best regards,
Autism Center Team
";
    }

    private static string GenerateCancellationEmailBody(Appointment appointment)
    {
        return $@"
Dear {appointment.User.FirstName} {appointment.User.LastName},

Your appointment has been cancelled:

Appointment Number: {appointment.AppointmentNumber}
Patient: {appointment.PatientInfo.PatientName}
Doctor: {appointment.Doctor?.NameEn}
Original Date & Time: {appointment.AppointmentDate:dddd, MMMM dd, yyyy 'at' h:mm tt}

If you would like to schedule a new appointment, please contact us or use our online booking system.

Best regards,
Autism Center Team
";
    }

    private static string GenerateRescheduledEmailBody(Appointment appointment, DateTime oldDate)
    {
        return $@"
Dear {appointment.User.FirstName} {appointment.User.LastName},

Your appointment has been rescheduled:

Appointment Number: {appointment.AppointmentNumber}
Patient: {appointment.PatientInfo.PatientName}
Doctor: {appointment.Doctor?.NameEn}
Previous Date & Time: {oldDate:dddd, MMMM dd, yyyy 'at' h:mm tt}
New Date & Time: {appointment.AppointmentDate:dddd, MMMM dd, yyyy 'at' h:mm tt}
Duration: {appointment.DurationInMinutes} minutes

{(appointment.HasZoomMeeting() ? $"Zoom Meeting Link: {appointment.ZoomJoinUrl}" : "")}

Please make note of the new appointment time.

Best regards,
Autism Center Team
";
    }

    private static string GenerateStartingSoonEmailBody(Appointment appointment)
    {
        return $@"
Dear {appointment.User.FirstName} {appointment.User.LastName},

Your appointment is starting soon:

Appointment Number: {appointment.AppointmentNumber}
Patient: {appointment.PatientInfo.PatientName}
Doctor: {appointment.Doctor?.NameEn}
Time: {appointment.AppointmentDate:h:mm tt}

{(appointment.HasZoomMeeting() ? $"Join Zoom Meeting: {appointment.ZoomJoinUrl}" : "")}

Please join the meeting now.

Best regards,
Autism Center Team
";
    }
}