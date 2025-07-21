using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Events;

public record CartCreatedEvent(Guid CartId, Guid UserId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}