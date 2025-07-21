using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Events;

public record CartItemAddedEvent(Guid CartId, Guid ProductId, int Quantity) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}