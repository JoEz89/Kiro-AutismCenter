using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Events;

public record OrderDeliveredEvent(Guid OrderId, string OrderNumber, DateTime DeliveredAt) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}