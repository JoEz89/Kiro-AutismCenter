using MediatR;

namespace AutismCenter.Application.Features.Courses.Queries.GetCourseCompletionStatus;

public record GetCourseCompletionStatusQuery(Guid EnrollmentId) : IRequest<GetCourseCompletionStatusResponse>;