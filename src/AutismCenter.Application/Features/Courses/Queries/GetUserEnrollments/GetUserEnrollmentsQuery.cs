using MediatR;

namespace AutismCenter.Application.Features.Courses.Queries.GetUserEnrollments;

public record GetUserEnrollmentsQuery(
    Guid UserId,
    bool ActiveOnly = false,
    bool IncludeExpired = true
) : IRequest<GetUserEnrollmentsResponse>;