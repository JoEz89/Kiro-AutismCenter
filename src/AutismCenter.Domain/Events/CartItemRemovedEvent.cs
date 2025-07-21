using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Events;

public record CartItemRemovedEvent(Guid CartId, Guid ProductId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}