using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Services;
using MediatR;

namespace AutismCenter.Application.Features.Appointments.Commands.CreateZoomMeeting;

public class CreateZoomMeetingHandler : IRequestHandler<CreateZoomMeetingCommand, CreateZoomMeetingResponse>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAppointmentZoomService _appointmentZoomService;

    public CreateZoomMeetingHandler(IAppointmentRepository appointmentRepository, IAppointmentZoomService appointmentZoomService)
    {
        _appointmentRepository = appointmentRepository;
        _appointmentZoomService = appointmentZoomService;
    }

    public async Task<CreateZoomMeetingResponse> Handle(CreateZoomMeetingCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment == null)
            throw new InvalidOperationException($"Appointment with ID {request.AppointmentId} not found");

        var joinUrl = await _appointmentZoomService.CreateMeetingForAppointmentAsync(appointment, cancellationToken);

        return new CreateZoomMeetingResponse(
            appointment.Id,
            appointment.ZoomMeetingId!,
            joinUrl,
            $"Appointment with {appointment.PatientInfo.PatientName}",
            appointment.AppointmentDate,
            appointment.DurationInMinutes);
    }
}