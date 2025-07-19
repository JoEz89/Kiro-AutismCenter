using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Events;

public record OrderPaymentFailedEvent(Guid OrderId, string OrderNumber) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}