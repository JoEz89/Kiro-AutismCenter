using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Events;

public record AppointmentStartedEvent(Guid AppointmentId, Guid UserId, Guid DoctorId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}