using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Events;

public record CartItemQuantityUpdatedEvent(Guid CartId, Guid ProductId, int OldQuantity, int NewQuantity) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}