using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Events;

public record ProductActivatedEvent(Guid ProductId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}