using MediatR;

namespace AutismCenter.Application.Features.Courses.Queries.Admin.GetCourseAnalytics;

public record GetCourseAnalyticsQuery(
    DateTime? StartDate,
    DateTime? EndDate,
    Guid? CategoryId
) : IRequest<GetCourseAnalyticsResponse>;