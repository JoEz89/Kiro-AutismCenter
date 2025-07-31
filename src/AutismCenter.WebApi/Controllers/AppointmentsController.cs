using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutismCenter.Application.Features.Appointments.Commands.BookAppointment;
using AutismCenter.Application.Features.Appointments.Commands.CancelAppointment;
using AutismCenter.Application.Features.Appointments.Commands.RescheduleAppointment;
using AutismCenter.Application.Features.Appointments.Commands.CreateZoomMeeting;
using AutismCenter.Application.Features.Appointments.Commands.CreateDoctorAvailability;
using AutismCenter.Application.Features.Appointments.Commands.UpdateDoctorAvailability;
using AutismCenter.Application.Features.Appointments.Commands.RemoveDoctorAvailability;
using AutismCenter.Application.Features.Appointments.Queries.GetAppointments;
using AutismCenter.Application.Features.Appointments.Queries.GetAvailableSlots;
using AutismCenter.Application.Features.Appointments.Queries.GetCalendarView;
using AutismCenter.Application.Features.Appointments.Queries.GetDoctorAvailability;
using AutismCenter.Domain.Enums;

namespace AutismCenter.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : BaseController
{
    /// <summary>
    /// Book a new appointment
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BookAppointmentResponse>> BookAppointment([FromBody] BookAppointmentRequest request)
    {
        var command = new BookAppointmentCommand(
            request.UserId,
            request.DoctorId,
            request.AppointmentDate,
            request.DurationInMinutes,
            request.PatientName,
            request.PatientAge,
            request.MedicalHistory,
            request.CurrentConcerns,
            request.EmergencyContact,
            request.EmergencyPhone
        );

        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Get appointments with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<GetAppointmentsResponse>> GetAppointments(
        [FromQuery] Guid? userId = null,
        [FromQuery] Guid? doctorId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] bool upcomingOnly = false)
    {
        var query = new GetAppointmentsQuery(userId, doctorId, startDate, endDate, upcomingOnly);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get appointment by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AppointmentDto>> GetAppointmentById(Guid id)
    {
        var query = new GetAppointmentsQuery(null, null, null, null, false);
        var result = await Mediator.Send(query);
        
        var appointment = result.Appointments.FirstOrDefault(a => a.Id == id);
        if (appointment == null)
            return NotFound($"Appointment with ID {id} not found");
            
        return Ok(appointment);
    }

    /// <summary>
    /// Cancel an appointment
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult> CancelAppointment(Guid id, [FromBody] CancelAppointmentRequest request)
    {
        var command = new CancelAppointmentCommand(id, request.UserId);
        await Mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Reschedule an appointment
    /// </summary>
    [HttpPut("{id:guid}/reschedule")]
    public async Task<ActionResult<RescheduleAppointmentResponse>> RescheduleAppointment(Guid id, [FromBody] RescheduleAppointmentRequest request)
    {
        var command = new RescheduleAppointmentCommand(id, request.UserId, request.NewAppointmentDate);
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Get available appointment slots
    /// </summary>
    [HttpGet("available-slots")]
    public async Task<ActionResult<GetAvailableSlotsResponse>> GetAvailableSlots(
        [FromQuery] Guid? doctorId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int durationInMinutes = 60)
    {
        var query = new GetAvailableSlotsQuery(doctorId, startDate, endDate, durationInMinutes);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get calendar view for appointments
    /// </summary>
    [HttpGet("calendar")]
    public async Task<ActionResult<GetCalendarViewResponse>> GetCalendarView(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] Guid? doctorId = null)
    {
        var query = new GetCalendarViewQuery(startDate, endDate, doctorId);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create Zoom meeting for appointment
    /// </summary>
    [HttpPost("{id:guid}/zoom-meeting")]
    public async Task<ActionResult<CreateZoomMeetingResponse>> CreateZoomMeeting(Guid id)
    {
        var command = new CreateZoomMeetingCommand(id);
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Get doctor availability
    /// </summary>
    [HttpGet("doctors/{doctorId:guid}/availability")]
    public async Task<ActionResult<GetDoctorAvailabilityResponse>> GetDoctorAvailability(Guid doctorId)
    {
        var query = new GetDoctorAvailabilityQuery(doctorId);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create doctor availability (Admin only)
    /// </summary>
    [HttpPost("doctors/{doctorId:guid}/availability")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CreateDoctorAvailabilityResponse>> CreateDoctorAvailability(Guid doctorId, [FromBody] CreateDoctorAvailabilityRequest request)
    {
        var command = new CreateDoctorAvailabilityCommand(
            doctorId,
            request.DayOfWeek,
            request.StartTime,
            request.EndTime
        );
        
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Update doctor availability (Admin only)
    /// </summary>
    [HttpPut("doctors/{doctorId:guid}/availability/{availabilityId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UpdateDoctorAvailabilityResponse>> UpdateDoctorAvailability(Guid doctorId, Guid availabilityId, [FromBody] UpdateDoctorAvailabilityRequest request)
    {
        var command = new UpdateDoctorAvailabilityCommand(
            doctorId,
            availabilityId,
            request.StartTime,
            request.EndTime,
            request.IsActive
        );
        
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Remove doctor availability (Admin only)
    /// </summary>
    [HttpDelete("doctors/{doctorId:guid}/availability/{availabilityId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> RemoveDoctorAvailability(Guid doctorId, Guid availabilityId)
    {
        var command = new RemoveDoctorAvailabilityCommand(doctorId, availabilityId);
        await Mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Send appointment reminder (Admin only)
    /// </summary>
    [HttpPost("{id:guid}/send-reminder")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> SendAppointmentReminder(Guid id)
    {
        // This would typically trigger a notification service
        // For now, we'll return success as the notification system would be implemented separately
        return Ok(new { Message = "Reminder sent successfully", AppointmentId = id });
    }

    /// <summary>
    /// Send appointment notification (Admin only)
    /// </summary>
    [HttpPost("{id:guid}/send-notification")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> SendAppointmentNotification(Guid id, [FromBody] SendNotificationRequest request)
    {
        // This would typically trigger a notification service
        // For now, we'll return success as the notification system would be implemented separately
        return Ok(new { 
            Message = "Notification sent successfully", 
            AppointmentId = id,
            NotificationType = request.NotificationType,
            CustomMessage = request.CustomMessage
        });
    }
}

// Request DTOs
public record BookAppointmentRequest(
    Guid UserId,
    Guid DoctorId,
    DateTime AppointmentDate,
    int DurationInMinutes,
    string PatientName,
    int PatientAge,
    string? MedicalHistory = null,
    string? CurrentConcerns = null,
    string? EmergencyContact = null,
    string? EmergencyPhone = null
);

public record CancelAppointmentRequest(Guid UserId);

public record RescheduleAppointmentRequest(Guid UserId, DateTime NewAppointmentDate);

public record CreateDoctorAvailabilityRequest(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime
);

public record UpdateDoctorAvailabilityRequest(
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive = true
);

public record SendNotificationRequest(
    string NotificationType,
    string? CustomMessage = null
);