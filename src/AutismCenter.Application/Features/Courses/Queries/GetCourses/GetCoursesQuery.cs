using MediatR;

namespace AutismCenter.Application.Features.Courses.Queries.GetCourses;

public record GetCoursesQuery(
    bool ActiveOnly = true,
    string? SearchTerm = null
) : IRequest<GetCoursesResponse>;