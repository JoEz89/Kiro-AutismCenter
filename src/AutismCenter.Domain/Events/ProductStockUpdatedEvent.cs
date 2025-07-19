using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Events;

public record ProductStockUpdatedEvent(Guid ProductId, int OldQuantity, int NewQuantity) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}