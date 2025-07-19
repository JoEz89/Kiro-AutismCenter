using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Events;

public record EnrollmentExtendedEvent(Guid EnrollmentId, Guid UserId, Guid CourseId, DateTime NewExpiryDate) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}