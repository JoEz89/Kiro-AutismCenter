using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Events;

public record CartClearedEvent(Guid CartId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}