using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Events;

public record OrderShippedEvent(Guid OrderId, string OrderNumber, DateTime ShippedAt) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}