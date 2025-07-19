using AutismCenter.Domain.Common;
using AutismCenter.Domain.ValueObjects;

namespace AutismCenter.Domain.Events;

public record OrderConfirmedEvent(Guid OrderId, string OrderNumber, Guid UserId, Money TotalAmount) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}