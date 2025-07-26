using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Services;
using MediatR;

namespace AutismCenter.Application.Features.Appointments.Commands.RescheduleAppointment;

public class RescheduleAppointmentHandler : IRequestHandler<RescheduleAppointmentCommand, RescheduleAppointmentResponse>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAppointmentSlotService _appointmentSlotService;
    private readonly IAppointmentZoomService _appointmentZoomService;

    public RescheduleAppointmentHandler(
        IAppointmentRepository appointmentRepository,
        IAppointmentSlotService appointmentSlotService,
        IAppointmentZoomService appointmentZoomService)
    {
        _appointmentRepository = appointmentRepository;
        _appointmentSlotService = appointmentSlotService;
        _appointmentZoomService = appointmentZoomService;
    }

    public async Task<RescheduleAppointmentResponse> Handle(RescheduleAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment == null)
            throw new InvalidOperationException($"Appointment with ID {request.AppointmentId} not found");

        // Verify the user owns this appointment
        if (appointment.UserId != request.UserId)
            throw new UnauthorizedAccessException("You can only reschedule your own appointments");

        // Check if appointment can be rescheduled
        if (!appointment.CanBeRescheduled())
            throw new InvalidOperationException($"Cannot reschedule appointment with status {appointment.Status}");

        // Validate new appointment slot is available
        await _appointmentSlotService.ValidateAppointmentSlotAsync(
            appointment.DoctorId, request.NewAppointmentDate, appointment.DurationInMinutes, 
            appointment.Id, cancellationToken);

        var oldDate = appointment.AppointmentDate;

        // Reschedule the appointment
        appointment.Reschedule(request.NewAppointmentDate);

        // Update Zoom meeting
        await _appointmentZoomService.HandleAppointmentRescheduledAsync(appointment, cancellationToken);

        // Save changes
        await _appointmentRepository.UpdateAsync(appointment, cancellationToken);

        return new RescheduleAppointmentResponse(
            appointment.Id,
            appointment.AppointmentNumber,
            oldDate,
            appointment.AppointmentDate,
            appointment.Status,
            appointment.ZoomJoinUrl);
    }
}