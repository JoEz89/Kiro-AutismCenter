using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Events;

public record CourseCreatedEvent(Guid CourseId, string CourseTitle) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}