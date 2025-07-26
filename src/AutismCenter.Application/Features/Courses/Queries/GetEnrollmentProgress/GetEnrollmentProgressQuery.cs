using MediatR;

namespace AutismCenter.Application.Features.Courses.Queries.GetEnrollmentProgress;

public record GetEnrollmentProgressQuery(Guid EnrollmentId) : IRequest<GetEnrollmentProgressResponse>;