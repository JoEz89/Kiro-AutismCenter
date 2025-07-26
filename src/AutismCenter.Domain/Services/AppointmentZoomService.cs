using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;

namespace AutismCenter.Domain.Services;

public class AppointmentZoomService : IAppointmentZoomService
{
    private readonly IZoomService _zoomService;
    private readonly IAppointmentRepository _appointmentRepository;

    public AppointmentZoomService(IZoomService zoomService, IAppointmentRepository appointmentRepository)
    {
        _zoomService = zoomService;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<string> CreateMeetingForAppointmentAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        if (appointment.HasZoomMeeting())
            throw new InvalidOperationException("Appointment already has a Zoom meeting");

        var meetingRequest = new ZoomMeetingRequest(
            Topic: $"Appointment with {appointment.PatientInfo.PatientName}",
            StartTime: appointment.AppointmentDate,
            DurationInMinutes: appointment.DurationInMinutes,
            Password: GenerateSecurePassword(),
            WaitingRoom: true,
            JoinBeforeHost: false,
            Agenda: $"Consultation appointment for {appointment.PatientInfo.PatientName}"
        );

        var meeting = await _zoomService.CreateMeetingAsync(meetingRequest, cancellationToken);
        
        appointment.SetZoomMeeting(meeting.Id, meeting.JoinUrl);
        await _appointmentRepository.UpdateAsync(appointment, cancellationToken);

        return meeting.JoinUrl;
    }

    public async Task UpdateMeetingForAppointmentAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        if (!appointment.HasZoomMeeting())
            throw new InvalidOperationException("Appointment does not have a Zoom meeting");

        var meetingRequest = new ZoomMeetingRequest(
            Topic: $"Appointment with {appointment.PatientInfo.PatientName}",
            StartTime: appointment.AppointmentDate,
            DurationInMinutes: appointment.DurationInMinutes,
            Password: null, // Keep existing password
            WaitingRoom: true,
            JoinBeforeHost: false,
            Agenda: $"Consultation appointment for {appointment.PatientInfo.PatientName}"
        );

        await _zoomService.UpdateMeetingAsync(appointment.ZoomMeetingId!, meetingRequest, cancellationToken);
    }

    public async Task DeleteMeetingForAppointmentAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        if (!appointment.HasZoomMeeting())
            return; // Nothing to delete

        try
        {
            await _zoomService.DeleteMeetingAsync(appointment.ZoomMeetingId!, cancellationToken);
        }
        catch (Exception)
        {
            // Log but don't throw - the meeting might already be deleted
            // We'll still clear the meeting info from the appointment
        }

        // Clear the meeting info from appointment
        appointment.ClearZoomMeeting();
        await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
    }

    public async Task<ZoomMeeting?> GetMeetingForAppointmentAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        if (!appointment.HasZoomMeeting())
            return null;

        try
        {
            return await _zoomService.GetMeetingAsync(appointment.ZoomMeetingId!, cancellationToken);
        }
        catch (Exception)
        {
            // Meeting might not exist anymore
            return null;
        }
    }

    public async Task HandleAppointmentRescheduledAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        if (!appointment.HasZoomMeeting())
        {
            // Create new meeting for rescheduled appointment
            await CreateMeetingForAppointmentAsync(appointment, cancellationToken);
        }
        else
        {
            // Update existing meeting
            await UpdateMeetingForAppointmentAsync(appointment, cancellationToken);
        }
    }

    public async Task HandleAppointmentCancelledAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        if (appointment.HasZoomMeeting())
        {
            await DeleteMeetingForAppointmentAsync(appointment, cancellationToken);
        }
    }

    private static string GenerateSecurePassword()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}