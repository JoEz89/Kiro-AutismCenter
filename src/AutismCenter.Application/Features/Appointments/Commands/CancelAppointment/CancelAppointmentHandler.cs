using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Services;
using MediatR;

namespace AutismCenter.Application.Features.Appointments.Commands.CancelAppointment;

public class CancelAppointmentHandler : IRequestHandler<CancelAppointmentCommand>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAppointmentZoomService _appointmentZoomService;

    public CancelAppointmentHandler(IAppointmentRepository appointmentRepository, IAppointmentZoomService appointmentZoomService)
    {
        _appointmentRepository = appointmentRepository;
        _appointmentZoomService = appointmentZoomService;
    }

    public async Task Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment == null)
            throw new InvalidOperationException($"Appointment with ID {request.AppointmentId} not found");

        // Verify the user owns this appointment
        if (appointment.UserId != request.UserId)
            throw new UnauthorizedAccessException("You can only cancel your own appointments");

        // Check if appointment can be cancelled
        if (!appointment.CanBeCancelled())
            throw new InvalidOperationException($"Cannot cancel appointment with status {appointment.Status}");

        // Cancel the appointment
        appointment.Cancel();

        // Cancel Zoom meeting
        await _appointmentZoomService.HandleAppointmentCancelledAsync(appointment, cancellationToken);

        // Save changes
        await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
    }
}