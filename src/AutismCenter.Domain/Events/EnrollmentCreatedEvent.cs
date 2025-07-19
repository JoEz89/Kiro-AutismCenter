using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Events;

public record EnrollmentCreatedEvent(Guid EnrollmentId, Guid UserId, Guid CourseId, DateTime EnrollmentDate, DateTime ExpiryDate) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}