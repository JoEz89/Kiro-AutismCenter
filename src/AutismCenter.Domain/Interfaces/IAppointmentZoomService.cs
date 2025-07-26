using AutismCenter.Domain.Entities;

namespace AutismCenter.Domain.Interfaces;

public interface IAppointmentZoomService
{
    Task<string> CreateMeetingForAppointmentAsync(Appointment appointment, CancellationToken cancellationToken = default);
    Task UpdateMeetingForAppointmentAsync(Appointment appointment, CancellationToken cancellationToken = default);
    Task DeleteMeetingForAppointmentAsync(Appointment appointment, CancellationToken cancellationToken = default);
    Task<ZoomMeeting?> GetMeetingForAppointmentAsync(Appointment appointment, CancellationToken cancellationToken = default);
    Task HandleAppointmentRescheduledAsync(Appointment appointment, CancellationToken cancellationToken = default);
    Task HandleAppointmentCancelledAsync(Appointment appointment, CancellationToken cancellationToken = default);
}