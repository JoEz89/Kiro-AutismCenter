namespace AutismCenter.Domain.ValueObjects;

public record EnrollmentStats(
    Guid CourseId,
    int EnrollmentCount,
    int CompletionCount,
    decimal Revenue
);