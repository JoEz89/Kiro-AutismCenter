using AutismCenter.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AutismCenter.Infrastructure.Services;

public class ZoomWebhookService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ILogger<ZoomWebhookService> _logger;

    public ZoomWebhookService(IAppointmentRepository appointmentRepository, ILogger<ZoomWebhookService> logger)
    {
        _appointmentRepository = appointmentRepository;
        _logger = logger;
    }

    public async Task HandleWebhookAsync(string payload, CancellationToken cancellationToken = default)
    {
        try
        {
            var webhookData = JsonSerializer.Deserialize<ZoomWebhookPayload>(payload);
            if (webhookData == null)
            {
                _logger.LogWarning("Invalid webhook payload received");
                return;
            }

            await ProcessWebhookEventAsync(webhookData, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Zoom webhook");
            throw;
        }
    }

    private async Task ProcessWebhookEventAsync(ZoomWebhookPayload payload, CancellationToken cancellationToken)
    {
        switch (payload.@event)
        {
            case "meeting.started":
                await HandleMeetingStartedAsync(payload.payload.@object, cancellationToken);
                break;
            case "meeting.ended":
                await HandleMeetingEndedAsync(payload.payload.@object, cancellationToken);
                break;
            case "meeting.participant_joined":
                await HandleParticipantJoinedAsync(payload.payload.@object, cancellationToken);
                break;
            case "meeting.participant_left":
                await HandleParticipantLeftAsync(payload.payload.@object, cancellationToken);
                break;
            default:
                _logger.LogInformation("Unhandled webhook event: {Event}", payload.@event);
                break;
        }
    }

    private async Task HandleMeetingStartedAsync(ZoomMeetingObject meetingObject, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetAllAsync(cancellationToken);
        var appointment = appointments.FirstOrDefault(a => a.ZoomMeetingId == meetingObject.id.ToString());

        if (appointment != null)
        {
            appointment.Start();
            await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
            _logger.LogInformation("Appointment {AppointmentId} started via Zoom meeting {MeetingId}", 
                appointment.Id, meetingObject.id);
        }
    }

    private async Task HandleMeetingEndedAsync(ZoomMeetingObject meetingObject, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetAllAsync(cancellationToken);
        var appointment = appointments.FirstOrDefault(a => a.ZoomMeetingId == meetingObject.id.ToString());

        if (appointment != null)
        {
            appointment.Complete($"Meeting ended at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
            _logger.LogInformation("Appointment {AppointmentId} completed via Zoom meeting {MeetingId}", 
                appointment.Id, meetingObject.id);
        }
    }

    private async Task HandleParticipantJoinedAsync(ZoomMeetingObject meetingObject, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Participant joined Zoom meeting {MeetingId}", meetingObject.id);
        // Additional logic can be added here if needed
        await Task.CompletedTask;
    }

    private async Task HandleParticipantLeftAsync(ZoomMeetingObject meetingObject, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Participant left Zoom meeting {MeetingId}", meetingObject.id);
        // Additional logic can be added here if needed
        await Task.CompletedTask;
    }

    // Internal DTOs for Zoom webhook payloads
    private record ZoomWebhookPayload(
        string @event,
        ZoomWebhookPayloadData payload
    );

    private record ZoomWebhookPayloadData(
        string account_id,
        ZoomMeetingObject @object
    );

    private record ZoomMeetingObject(
        long id,
        string uuid,
        string host_id,
        string topic,
        int type,
        string start_time,
        string timezone,
        int duration,
        ZoomParticipant? participant
    );

    private record ZoomParticipant(
        string user_id,
        string user_name,
        string id,
        string join_time,
        string leave_time
    );
}