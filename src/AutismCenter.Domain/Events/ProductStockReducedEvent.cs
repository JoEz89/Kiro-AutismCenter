using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Events;

public record ProductStockReducedEvent(Guid ProductId, int QuantityReduced, int RemainingStock) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}