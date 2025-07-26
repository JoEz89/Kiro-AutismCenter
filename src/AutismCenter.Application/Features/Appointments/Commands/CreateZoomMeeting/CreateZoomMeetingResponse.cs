namespace AutismCenter.Application.Features.Appointments.Commands.CreateZoomMeeting;

public record CreateZoomMeetingResponse(
    Guid AppointmentId,
    string MeetingId,
    string JoinUrl,
    string Topic,
    DateTime StartTime,
    int DurationInMinutes
);