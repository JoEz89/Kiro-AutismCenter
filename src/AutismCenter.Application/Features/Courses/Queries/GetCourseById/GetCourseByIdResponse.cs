using AutismCenter.Application.Features.Courses.Common;

namespace AutismCenter.Application.Features.Courses.Queries.GetCourseById;

public record GetCourseByIdResponse(
    bool Success,
    string Message,
    CourseDto? Course = null,
    Dictionary<string, string[]>? Errors = null
);