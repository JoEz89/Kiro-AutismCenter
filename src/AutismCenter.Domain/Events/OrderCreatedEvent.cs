using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Events;

public record OrderCreatedEvent(Guid OrderId, string OrderNumber, Guid UserId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}