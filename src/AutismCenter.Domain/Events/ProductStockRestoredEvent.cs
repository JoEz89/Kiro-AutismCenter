using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Events;

public record ProductStockRestoredEvent(Guid ProductId, int QuantityRestored, int NewStock) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}