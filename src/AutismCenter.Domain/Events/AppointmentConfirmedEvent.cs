using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Events;

public record AppointmentConfirmedEvent(Guid AppointmentId, Guid UserId, Guid DoctorId, DateTime AppointmentDate) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}