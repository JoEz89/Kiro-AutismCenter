using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Events;

public record AppointmentZoomMeetingCreatedEvent(Guid AppointmentId, string MeetingId, string JoinUrl) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}