namespace AutismCenter.Domain.Exceptions;

public class CourseAccessDeniedException : DomainException
{
    public Guid UserId { get; }
    public Guid CourseId { get; }

    public CourseAccessDeniedException(Guid userId, Guid courseId) 
        : base($"User '{userId}' does not have access to course '{courseId}'")
    {
        UserId = userId;
        CourseId = courseId;
    }
}