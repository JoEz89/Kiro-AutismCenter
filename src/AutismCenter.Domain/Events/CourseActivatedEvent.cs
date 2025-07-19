using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Events;

public record CourseActivatedEvent(Guid CourseId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}