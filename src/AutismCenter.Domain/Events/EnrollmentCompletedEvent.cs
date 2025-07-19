using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Events;

public record EnrollmentCompletedEvent(Guid EnrollmentId, Guid UserId, Guid CourseId, DateTime CompletionDate) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}