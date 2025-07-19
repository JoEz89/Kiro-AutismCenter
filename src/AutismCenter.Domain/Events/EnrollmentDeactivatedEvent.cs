using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Events;

public record EnrollmentDeactivatedEvent(Guid EnrollmentId, Guid UserId, Guid CourseId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}