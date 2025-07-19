using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Events;

public record CertificateGeneratedEvent(Guid EnrollmentId, Guid UserId, Guid CourseId, string CertificateUrl) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}