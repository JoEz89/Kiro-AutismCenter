using AutismCenter.Application.Features.Courses.Common;

namespace AutismCenter.Application.Features.Courses.Queries.GetCourses;

public record GetCoursesResponse(
    bool Success,
    string Message,
    IEnumerable<CourseSummaryDto> Courses,
    Dictionary<string, string[]>? Errors = null
);