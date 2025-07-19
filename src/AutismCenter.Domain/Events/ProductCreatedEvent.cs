using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Events;

public record ProductCreatedEvent(Guid ProductId, string ProductName) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}