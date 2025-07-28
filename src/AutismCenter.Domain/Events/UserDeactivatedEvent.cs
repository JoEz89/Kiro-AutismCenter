using AutismCenter.Domain.Common;
using AutismCenter.Domain.ValueObjects;

namespace AutismCenter.Domain.Events;

public record UserDeactivatedEvent(Guid UserId, Email Email) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}