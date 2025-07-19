using AutismCenter.Domain.Common;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Domain.Events;

public record UserRoleChangedEvent(Guid UserId, UserRole OldRole, UserRole NewRole) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}