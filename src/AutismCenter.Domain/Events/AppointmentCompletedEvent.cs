using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Events;

public record AppointmentCompletedEvent(Guid AppointmentId, Guid UserId, Guid DoctorId, DateTime CompletedAt) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}