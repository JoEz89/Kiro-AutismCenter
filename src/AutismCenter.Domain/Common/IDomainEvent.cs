namespace AutismCenter.Domain.Common;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}