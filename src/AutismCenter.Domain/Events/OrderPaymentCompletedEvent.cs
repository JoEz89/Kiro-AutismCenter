using AutismCenter.Domain.Common;
using AutismCenter.Domain.ValueObjects;

namespace AutismCenter.Domain.Events;

public record OrderPaymentCompletedEvent(Guid OrderId, string OrderNumber, string PaymentId, Money Amount) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}