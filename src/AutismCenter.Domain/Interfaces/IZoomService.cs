using AutismCenter.Domain.ValueObjects;

namespace AutismCenter.Domain.Interfaces;

public interface IZoomService
{
    Task<ZoomMeeting> CreateMeetingAsync(ZoomMeetingRequest request, CancellationToken cancellationToken = default);
    Task<ZoomMeeting> GetMeetingAsync(string meetingId, CancellationToken cancellationToken = default);
    Task UpdateMeetingAsync(string meetingId, ZoomMeetingRequest request, CancellationToken cancellationToken = default);
    Task DeleteMeetingAsync(string meetingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ZoomMeeting>> GetUserMeetingsAsync(string userId, CancellationToken cancellationToken = default);
}

public record ZoomMeetingRequest(
    string Topic,
    DateTime StartTime,
    int DurationInMinutes,
    string? Password = null,
    bool WaitingRoom = true,
    bool JoinBeforeHost = false,
    string? Agenda = null
);

public record ZoomMeeting(
    string Id,
    string Topic,
    string JoinUrl,
    string StartUrl,
    DateTime StartTime,
    int DurationInMinutes,
    string? Password,
    ZoomMeetingStatus Status
);

public enum ZoomMeetingStatus
{
    Waiting = 0,
    Started = 1,
    Ended = 2
}